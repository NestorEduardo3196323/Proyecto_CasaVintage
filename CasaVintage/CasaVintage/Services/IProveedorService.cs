using CasaVintage.Models;
using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Motivo por el que una operacion sobre un proveedor no se pudo completar.
    public enum ErrorProveedor
    {
        Ninguno,
        NoEncontrado,
        TieneProductos
    }

    // Resultado de una operacion de escritura sobre un proveedor: exito y, si fallo, el motivo.
    public sealed record ResultadoProveedor(bool Exito, ErrorProveedor Error = ErrorProveedor.Ninguno, Proveedor? Proveedor = null)
    {
        public static ResultadoProveedor Ok(Proveedor proveedor) => new(true, ErrorProveedor.Ninguno, proveedor);
        public static ResultadoProveedor Falla(ErrorProveedor error) => new(false, error, null);
    }

    // Contrato del modulo de Proveedores (Admin/Gerente). Concentra la logica de negocio; en
    // particular, no permite eliminar un proveedor que tenga productos asociados (FK Restrict).
    public interface IProveedorService
    {
        // Todos los proveedores, ordenados por nombre, con su numero de productos.
        Task<IReadOnlyList<ProveedorListItemViewModel>> ListarAsync();

        // Un proveedor por id (null si no existe).
        Task<Proveedor?> ObtenerAsync(int id);

        // Da de alta un proveedor. Contacto y telefono se guardan como null si vienen vacios.
        Task<ResultadoProveedor> CrearAsync(string nombre, string? contacto, string? telefono);

        // Actualiza los datos de un proveedor. Falla si no existe.
        Task<ResultadoProveedor> EditarAsync(int id, string nombre, string? contacto, string? telefono);

        // Elimina un proveedor. Falla si no existe o si tiene productos asociados (no se puede
        // romper el historial de inventario/ventas).
        Task<ResultadoProveedor> EliminarAsync(int id);
    }
}
