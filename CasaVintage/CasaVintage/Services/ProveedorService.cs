using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Suppliers module implementation. The business logic lives here so the PageModels only
    // orchestrate. Empty contact and phone are normalized to null.
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
            // Projection with the product count in a single query (avoids N+1).
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

            _logger.LogInformation("Supplier created: {Nombre} (id {Id}).", proveedor.Nombre, proveedor.IdProveedor);
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

            _logger.LogInformation("Supplier edited: id {Id} ({Nombre}).", proveedor.IdProveedor, proveedor.Nombre);
            return ResultadoProveedor.Ok(proveedor);
        }

        public async Task<ResultadoProveedor> EliminarAsync(int id)
        {
            var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return ResultadoProveedor.Falla(ErrorProveedor.NoEncontrado);
            }

            // Guard: a supplier with products cannot be deleted (FK Restrict); it would break the
            // inventory and sales history. It is blocked and reassigning/removing products is suggested.
            var tieneProductos = await _db.Productos.AnyAsync(p => p.IdProveedor == id);
            if (tieneProductos)
            {
                return ResultadoProveedor.Falla(ErrorProveedor.TieneProductos);
            }

            _db.Proveedores.Remove(proveedor);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Supplier deleted: id {Id} ({Nombre}).", id, proveedor.Nombre);
            return ResultadoProveedor.Ok(proveedor);
        }

        // Converts empty or whitespace-only strings to null (optional columns in the schema).
        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
