using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Implementacion del modulo de Proveedores. La logica de negocio vive aqui para que las
    // PageModels solo orquesten. Contacto y telefono vacios se normalizan a null.
    public class ProveedorService : IProveedorService
    {
        private readonly CasaVintageContext _db;
        private readonly ILogger<ProveedorService> _logger;

        public ProveedorService(CasaVintageContext db, ILogger<ProveedorService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProveedorListItemViewModel>> ListarAsync()
        {
            // Proyeccion con el conteo de productos en una sola consulta (evita N+1).
            return await _db.Proveedores
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorListItemViewModel(
                    p.IdProveedor,
                    p.Nombre,
                    p.Contacto,
                    p.Telefono,
                    p.Productos.Count))
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerAsync(int id)
        {
            return await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
        }

        public async Task<ResultadoProveedor> CrearAsync(string nombre, string? contacto, string? telefono)
        {
            var proveedor = new Proveedor
            {
                Nombre = nombre.Trim(),
                Contacto = Normalizar(contacto),
                Telefono = Normalizar(telefono)
            };

            _db.Proveedores.Add(proveedor);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Proveedor creado: {Nombre} (id {Id}).", proveedor.Nombre, proveedor.IdProveedor);
            return ResultadoProveedor.Ok(proveedor);
        }

        public async Task<ResultadoProveedor> EditarAsync(int id, string nombre, string? contacto, string? telefono)
        {
            var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return ResultadoProveedor.Falla(ErrorProveedor.NoEncontrado);
            }

            proveedor.Nombre = nombre.Trim();
            proveedor.Contacto = Normalizar(contacto);
            proveedor.Telefono = Normalizar(telefono);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Proveedor editado: id {Id} ({Nombre}).", proveedor.IdProveedor, proveedor.Nombre);
            return ResultadoProveedor.Ok(proveedor);
        }

        public async Task<ResultadoProveedor> EliminarAsync(int id)
        {
            var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return ResultadoProveedor.Falla(ErrorProveedor.NoEncontrado);
            }

            // Guarda: no se puede eliminar un proveedor con productos (FK Restrict); romperia el
            // historial de inventario y ventas. Se bloquea y se sugiere reasignar/quitar productos.
            var tieneProductos = await _db.Productos.AnyAsync(p => p.IdProveedor == id);
            if (tieneProductos)
            {
                return ResultadoProveedor.Falla(ErrorProveedor.TieneProductos);
            }

            _db.Proveedores.Remove(proveedor);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Proveedor eliminado: id {Id} ({Nombre}).", id, proveedor.Nombre);
            return ResultadoProveedor.Ok(proveedor);
        }

        // Convierte cadenas vacias o con solo espacios en null (columnas opcionales del esquema).
        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
