using CasaVintage.Models;
using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Reason an operation on a product could not be completed.
    public enum ErrorProducto
    {
        Ninguno,
        NoEncontrado,
        ProveedorInvalido,
        TieneVentas,
        Conflicto
    }

    // Result of a write operation on a product.
    public sealed record ResultadoProducto(bool Exito, ErrorProducto Error = ErrorProducto.Ninguno, Producto? Producto = null)
    {
        public static ResultadoProducto Ok(Producto producto) => new(true, ErrorProducto.Ninguno, producto);
        public static ResultadoProducto Falla(ErrorProducto error) => new(false, error, null);
    }

    // Product data to create/edit. The photos are paths already saved on disk (or null).
    // The SKU is not here: it is auto-generated on create and not modified on edit.
    public sealed record ProductoDatos(
        string Nombre,
        string Descripcion,
        string Epoca,
        string Estado,
        string Categoria,
        decimal Precio,
        decimal Costo,
        int Stock,
        int StockMinimo,
        int IdProveedor,
        string? Foto1,
        string? Foto2,
        string? Foto3);

    // An option of the suppliers <select>.
    public sealed record ProveedorOpcion(int Id, string Nombre);

    // Auxiliary data to build the form: suppliers and suggestions (already-used values).
    public sealed record ProductoFormData(
        IReadOnlyList<ProveedorOpcion> Proveedores,
        IReadOnlyList<string> Categorias,
        IReadOnlyList<string> Epocas,
        IReadOnlyList<string> Estados);

    // Contract of the Inventory/Products module (Admin/Manager). It concentrates: SKU auto-generation,
    // availability=(stock>0) sync, supplier validation and the delete guard (sales FK).
    public interface IProductoService
    {
        // All products for the inventory list (with supplier, thumbnail and whether it is deletable).
        Task<IReadOnlyList<ProductoListItemViewModel>> ListarAsync();

        // Products for the catalog (salesperson view): available first, with their photos.
        Task<IReadOnlyList<ProductoCatalogoViewModel>> ListarCatalogoAsync();

        // Detail card of a product for the catalog (with supplier and the 3 photos). Null if it does not exist.
        Task<ProductoDetalleViewModel?> ObtenerDetalleAsync(int id);

        // Catalog search (live search): filters by free text and by category/era/condition.
        Task<IReadOnlyList<ProductoCatalogoViewModel>> BuscarCatalogoAsync(string? texto, string? categoria, string? epoca, string? estado);

        // A single product by id (null if it does not exist).
        Task<Producto?> ObtenerAsync(int id);

        // Suppliers and suggestions for the create/edit forms.
        Task<ProductoFormData> ObtenerDatosFormularioAsync();

        // Creates the product with an auto-generated SKU and synced availability. Fails if the
        // supplier does not exist.
        Task<ResultadoProducto> CrearAsync(ProductoDatos datos);

        // Updates the product and resyncs availability. Fails if it does not exist, the supplier is
        // not valid, or there is a concurrency conflict.
        Task<ResultadoProducto> EditarAsync(int id, ProductoDatos datos);

        // Deletes the product (and its photos from disk). Fails if it has registered sales (FK Restrict).
        Task<ResultadoProducto> EliminarAsync(int id);
    }
}
