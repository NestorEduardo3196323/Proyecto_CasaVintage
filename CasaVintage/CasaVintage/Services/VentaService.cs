using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Processes the sale in a SINGLE transaction: inserts customer, sale and detail, decrements the
    // stock and syncs availability. It uses optimistic concurrency (products' row_version): if another
    // salesperson changed the stock between loading and saving, the update fails and everything is
    // rolled back (nobody sells the last unit twice). On success, it empties the cart.
    public class VentaService : IVentaService
    {
        private readonly CasaVintageContext _db;
        private readonly ICarritoService _carrito;
        private readonly ILogger<VentaService> _logger;

        public VentaService(CasaVintageContext db, ICarritoService carrito, ILogger<VentaService> logger)
        {
            _db = db;
            _carrito = carrito;
            _logger = logger;
        }

        public async Task<ResultadoVenta> ProcesarVentaAsync(string clienteNombre, string clienteCorreo, string metodoPago, int idUsuario, string? tarjetaUltimos4 = null)
        {
            metodoPago = (metodoPago ?? string.Empty).Trim();
            if (metodoPago != "Efectivo" && metodoPago != "Tarjeta")
            {
                return new ResultadoVenta(false, 0, ErrorVenta.DatosInvalidos);
            }

            // Only the last 4 digits are kept, and only when the payment is by card.
            tarjetaUltimos4 = metodoPago == "Tarjeta" ? tarjetaUltimos4 : null;

            var carrito = await _carrito.ObtenerAsync();
            if (carrito.Vacio)
            {
                return new ResultadoVenta(false, 0, ErrorVenta.CarritoVacio);
            }

            var cantidades = carrito.Items.ToDictionary(i => i.IdProducto, i => i.Cantidad);
            var ids = cantidades.Keys.ToList();

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Tracked products (to decrement stock and compare row_version when saving).
                var productos = await _db.Productos.Where(p => ids.Contains(p.IdProducto)).ToListAsync();

                // Validation against the current stock (fresh from the database).
                foreach (var par in cantidades)
                {
                    var producto = productos.FirstOrDefault(p => p.IdProducto == par.Key);
                    if (producto is null)
                    {
                        return new ResultadoVenta(false, 0, ErrorVenta.StockInsuficiente, "A cart product no longer exists.");
                    }
                    if (producto.Stock < par.Value)
                    {
                        return new ResultadoVenta(false, 0, ErrorVenta.StockInsuficiente, producto.Nombre);
                    }
                }

                // A new customer for each sale (no dedupe by email, project business rule).
                var cliente = new Cliente
                {
                    Nombre = clienteNombre.Trim(),
                    Correo = clienteCorreo.Trim()
                };

                var venta = new Venta
                {
                    Cliente = cliente,
                    IdUsuario = idUsuario,
                    MetodoPago = metodoPago,
                    TarjetaUltimos4 = tarjetaUltimos4
                };

                decimal total = 0;
                foreach (var par in cantidades)
                {
                    var producto = productos.First(p => p.IdProducto == par.Key);
                    venta.Detalles.Add(new DetalleVenta
                    {
                        Producto = producto,
                        Cantidad = par.Value,
                        PrecioUnitario = producto.Precio
                    });
                    total += producto.Precio * par.Value;

                    // Decrement stock and sync availability within the same transaction.
                    producto.Stock -= par.Value;
                    producto.Disponibilidad = producto.Stock > 0;
                }
                venta.TotalPagado = total;

                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _carrito.Vaciar();
                _logger.LogInformation("Sale {Id} registered by user {Usuario} (total {Total}).", venta.IdVenta, idUsuario, total);
                return new ResultadoVenta(true, venta.IdVenta);
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                _logger.LogWarning("Concurrency conflict while processing the sale (user {Usuario}).", idUsuario);
                return new ResultadoVenta(false, 0, ErrorVenta.Conflicto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error while processing the sale (user {Usuario}).", idUsuario);
                return new ResultadoVenta(false, 0, ErrorVenta.Error);
            }
        }

        public async Task<VentaConfirmacionViewModel?> ObtenerConfirmacionAsync(int idVenta)
        {
            return await _db.Ventas
                .AsNoTracking()
                .Where(v => v.IdVenta == idVenta)
                .Select(v => new VentaConfirmacionViewModel(
                    v.IdVenta,
                    v.Fecha,
                    v.Cliente!.Nombre,
                    v.Cliente.Correo,
                    v.MetodoPago,
                    v.TotalPagado,
                    v.Usuario!.NombreUsuario,
                    v.Detalles.Select(d => new VentaLineaViewModel(
                        d.Producto!.Sku,
                        d.Producto.Nombre,
                        d.Cantidad,
                        d.PrecioUnitario)).ToList(),
                    v.TarjetaUltimos4))
                .FirstOrDefaultAsync();
        }
    }
}
