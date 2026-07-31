using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Procesa la venta en UNA sola transaccion: inserta cliente, venta y detalle, descuenta el stock
    // y sincroniza la disponibilidad. Usa concurrencia optimista (row_version de productos): si otro
    // vendedor cambio el stock entre que se cargo y se guardo, la actualizacion falla y se revierte
    // todo (nadie vende la ultima unidad dos veces). Al terminar bien, vacia el carrito.
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

            // Solo se conservan los ultimos 4 digitos y unicamente cuando el pago es con tarjeta.
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
                // Productos con seguimiento (para descontar stock y comparar row_version al guardar).
                var productos = await _db.Productos.Where(p => ids.Contains(p.IdProducto)).ToListAsync();

                // Validacion con el stock actual (fresco de la base).
                foreach (var par in cantidades)
                {
                    var producto = productos.FirstOrDefault(p => p.IdProducto == par.Key);
                    if (producto is null)
                    {
                        return new ResultadoVenta(false, 0, ErrorVenta.StockInsuficiente, "Un producto del carrito ya no existe.");
                    }
                    if (producto.Stock < par.Value)
                    {
                        return new ResultadoVenta(false, 0, ErrorVenta.StockInsuficiente, producto.Nombre);
                    }
                }

                // Cliente nuevo por cada venta (no se dedupea por correo, RN del proyecto).
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

                    // Descontar stock y sincronizar disponibilidad dentro de la misma transaccion.
                    producto.Stock -= par.Value;
                    producto.Disponibilidad = producto.Stock > 0;
                }
                venta.TotalPagado = total;

                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _carrito.Vaciar();
                _logger.LogInformation("Venta {Id} registrada por usuario {Usuario} (total {Total}).", venta.IdVenta, idUsuario, total);
                return new ResultadoVenta(true, venta.IdVenta);
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                _logger.LogWarning("Conflicto de concurrencia al procesar la venta (usuario {Usuario}).", idUsuario);
                return new ResultadoVenta(false, 0, ErrorVenta.Conflicto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error al procesar la venta (usuario {Usuario}).", idUsuario);
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
