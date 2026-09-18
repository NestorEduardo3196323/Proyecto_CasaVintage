using CasaVintage.Models;
using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Reason an operation on a supplier could not be completed.
    public enum ErrorProveedor
    {
        Ninguno,
        NoEncontrado,
        TieneProductos
    }

    // Result of a write operation on a supplier: success and, if it failed, the reason.
    public sealed record ResultadoProveedor(bool Exito, ErrorProveedor Error = ErrorProveedor.Ninguno, Proveedor? Proveedor = null)
    {
        public static ResultadoProveedor Ok(Proveedor proveedor) => new(true, ErrorProveedor.Ninguno, proveedor);
        public static ResultadoProveedor Falla(ErrorProveedor error) => new(false, error, null);
    }

    // Contract of the Suppliers module (Admin/Manager). It concentrates the business logic; in
    // particular, it does not allow deleting a supplier that has associated products (FK Restrict).
    public interface IProveedorService
    {
        // All suppliers, ordered by name, with their number of products.
        Task<IReadOnlyList<ProveedorListItemViewModel>> ListarAsync();

        // A supplier by id (null if it does not exist).
        Task<Proveedor?> ObtenerAsync(int id);

        // Registers a supplier. Contact and phone are stored as null if they come in empty.
        Task<ResultadoProveedor> CrearAsync(string nombre, string? contacto, string? telefono);

        // Updates a supplier's data. Fails if it does not exist.
        Task<ResultadoProveedor> EditarAsync(int id, string nombre, string? contacto, string? telefono);

        // Deletes a supplier. Fails if it does not exist or if it has associated products (the
        // inventory/sales history cannot be broken).
        Task<ResultadoProveedor> EliminarAsync(int id);
    }
}
