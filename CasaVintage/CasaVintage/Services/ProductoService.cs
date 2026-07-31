using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Implementacion del modulo de Inventario/Productos. Concentra las reglas de negocio: el SKU se
    // autogenera (VIN-####), la disponibilidad se sincroniza con el stock en la misma operacion, y
    // no se puede borrar un producto que ya tiene ventas. Las fotos se guardan como rutas en disco.
    public class ProductoService : IProductoService
    {
        private const string PrefijoSku = "VIN-";

        private readonly CasaVintageContext _db;
        private readonly IAlmacenArchivos _archivos;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(CasaVintageContext db, IAlmacenArchivos archivos, ILogger<ProductoService> logger)
        {
            _db = db;
            _archivos = archivos;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductoListItemViewModel>> ListarAsync()
        {
            return await _db.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoListItemViewModel(
                    p.IdProducto,
                    p.Sku,
                    p.Nombre,
                    p.Categoria,
                    p.Epoca,
                    p.Precio,
                    p.Costo,
                    p.Stock,
                    p.StockMinimo,
                    p.Disponibilidad,
                    p.Proveedor!.Nombre,
                    p.Foto1,
                    !p.Detalles.Any()))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductoCatalogoViewModel>> ListarCatalogoAsync()
        {
            // Disponibles primero (con stock) y luego alfabetico: el vendedor ve lo vendible arriba.
            return await _db.Productos
                .AsNoTracking()
                .OrderByDescending(p => p.Disponibilidad)
                .ThenBy(p => p.Nombre)
                .Select(p => new ProductoCatalogoViewModel(
                    p.IdProducto,
                    p.Sku,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.Epoca,
                    p.Estado,
                    p.Categoria,
                    p.Stock,
                    p.Disponibilidad,
                    p.Foto1,
                    p.Foto2,
                    p.Foto3))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductoCatalogoViewModel>> BuscarCatalogoAsync(
            string? texto, string? categoria, string? epoca, string? estado)
        {
            var consulta = _db.Productos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var t = texto.Trim().ToLower();
                consulta = consulta.Where(p =>
                    p.Nombre.ToLower().Contains(t) ||
                    p.Sku.ToLower().Contains(t) ||
                    p.Categoria.ToLower().Contains(t) ||
                    p.Epoca.ToLower().Contains(t) ||
                    p.Descripcion.ToLower().Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                consulta = consulta.Where(p => p.Categoria == categoria);
            }
            if (!string.IsNullOrWhiteSpace(epoca))
            {
                consulta = consulta.Where(p => p.Epoca == epoca);
            }
            if (!string.IsNullOrWhiteSpace(estado))
            {
                consulta = consulta.Where(p => p.Estado == estado);
            }

            return await consulta
                .OrderByDescending(p => p.Disponibilidad)
                .ThenBy(p => p.Nombre)
                .Select(p => new ProductoCatalogoViewModel(
                    p.IdProducto,
                    p.Sku,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.Epoca,
                    p.Estado,
                    p.Categoria,
                    p.Stock,
                    p.Disponibilidad,
                    p.Foto1,
                    p.Foto2,
                    p.Foto3))
                .ToListAsync();
        }

        public async Task<ProductoDetalleViewModel?> ObtenerDetalleAsync(int id)
        {
            return await _db.Productos
                .AsNoTracking()
                .Where(p => p.IdProducto == id)
                .Select(p => new ProductoDetalleViewModel(
                    p.IdProducto,
                    p.Sku,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.Epoca,
                    p.Estado,
                    p.Categoria,
                    p.Stock,
                    p.Disponibilidad,
                    p.Proveedor!.Nombre,
                    p.Foto1,
                    p.Foto2,
                    p.Foto3))
                .FirstOrDefaultAsync();
        }

        public async Task<Producto?> ObtenerAsync(int id)
        {
            return await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
        }

        public async Task<ProductoFormData> ObtenerDatosFormularioAsync()
        {
            var proveedores = await _db.Proveedores
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorOpcion(p.IdProveedor, p.Nombre))
                .ToListAsync();

            // Sugerencias para los datalist: valores ya usados (el campo sigue siendo texto libre).
            var categorias = await ValoresDistintosAsync(p => p.Categoria);
            var epocas = await ValoresDistintosAsync(p => p.Epoca);
            var estados = await ValoresDistintosAsync(p => p.Estado);

            return new ProductoFormData(proveedores, categorias, epocas, estados);
        }

        public async Task<ResultadoProducto> CrearAsync(ProductoDatos datos)
        {
            if (!await _db.Proveedores.AnyAsync(p => p.IdProveedor == datos.IdProveedor))
            {
                return ResultadoProducto.Falla(ErrorProducto.ProveedorInvalido);
            }

            var producto = new Producto();
            AplicarDatos(producto, datos);

            // El SKU se autogenera. Si por concurrencia chocara con el indice unico, se reintenta.
            for (var intento = 0; intento < 3; intento++)
            {
                producto.Sku = await GenerarSkuAsync();
                _db.Productos.Add(producto);
                try
                {
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Producto creado: {Sku} ({Nombre}).", producto.Sku, producto.Nombre);
                    return ResultadoProducto.Ok(producto);
                }
                catch (DbUpdateException) when (intento < 2)
                {
                    // Probable colision de SKU: se descarta el intento y se genera otro.
                    _db.Entry(producto).State = EntityState.Detached;
                }
            }

            return ResultadoProducto.Falla(ErrorProducto.Conflicto);
        }

        public async Task<ResultadoProducto> EditarAsync(int id, ProductoDatos datos)
        {
            if (!await _db.Proveedores.AnyAsync(p => p.IdProveedor == datos.IdProveedor))
            {
                return ResultadoProducto.Falla(ErrorProducto.ProveedorInvalido);
            }

            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return ResultadoProducto.Falla(ErrorProducto.NoEncontrado);
            }

            AplicarDatos(producto, datos);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Otro usuario modifico el producto entre la carga y el guardado.
                return ResultadoProducto.Falla(ErrorProducto.Conflicto);
            }

            _logger.LogInformation("Producto editado: {Sku} (id {Id}).", producto.Sku, producto.IdProducto);
            return ResultadoProducto.Ok(producto);
        }

        public async Task<ResultadoProducto> EliminarAsync(int id)
        {
            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return ResultadoProducto.Falla(ErrorProducto.NoEncontrado);
            }

            // Guarda: no se puede borrar un producto que ya se vendio (FK Restrict con detalle_venta);
            // romperia el historial de ventas.
            if (await _db.DetallesVenta.AnyAsync(d => d.IdProducto == id))
            {
                return ResultadoProducto.Falla(ErrorProducto.TieneVentas);
            }

            _db.Productos.Remove(producto);
            await _db.SaveChangesAsync();

            // Ya sin fila que las referencie, se borran las fotos del disco.
            _archivos.Eliminar(producto.Foto1);
            _archivos.Eliminar(producto.Foto2);
            _archivos.Eliminar(producto.Foto3);

            _logger.LogInformation("Producto eliminado: {Sku} (id {Id}).", producto.Sku, id);
            return ResultadoProducto.Ok(producto);
        }

        // Copia los datos del formulario a la entidad y sincroniza disponibilidad = (stock > 0).
        private static void AplicarDatos(Producto producto, ProductoDatos datos)
        {
            producto.Nombre = datos.Nombre.Trim();
            producto.Descripcion = datos.Descripcion.Trim();
            producto.Epoca = datos.Epoca.Trim();
            producto.Estado = datos.Estado.Trim();
            producto.Categoria = datos.Categoria.Trim();
            producto.Precio = datos.Precio;
            producto.Costo = datos.Costo;
            producto.Stock = datos.Stock;
            producto.StockMinimo = datos.StockMinimo;
            producto.Disponibilidad = datos.Stock > 0;
            producto.IdProveedor = datos.IdProveedor;
            producto.Foto1 = datos.Foto1;
            producto.Foto2 = datos.Foto2;
            producto.Foto3 = datos.Foto3;
        }

        // Siguiente SKU correlativo con el prefijo VIN- (VIN-0001, VIN-0002, ...).
        private async Task<string> GenerarSkuAsync()
        {
            var skus = await _db.Productos
                .Where(p => p.Sku.StartsWith(PrefijoSku))
                .Select(p => p.Sku)
                .ToListAsync();

            var maximo = 0;
            foreach (var sku in skus)
            {
                if (int.TryParse(sku.AsSpan(PrefijoSku.Length), out var numero) && numero > maximo)
                {
                    maximo = numero;
                }
            }

            return $"{PrefijoSku}{maximo + 1:D4}";
        }

        private async Task<IReadOnlyList<string>> ValoresDistintosAsync(
            System.Linq.Expressions.Expression<Func<Producto, string>> selector)
        {
            return await _db.Productos
                .Select(selector)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();
        }
    }
}
