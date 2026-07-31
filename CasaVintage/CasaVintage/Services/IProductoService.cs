using CasaVintage.Models;
using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Motivo por el que una operacion sobre un producto no se pudo completar.
    public enum ErrorProducto
    {
        Ninguno,
        NoEncontrado,
        ProveedorInvalido,
        TieneVentas,
        Conflicto
    }

    // Resultado de una operacion de escritura sobre un producto.
    public sealed record ResultadoProducto(bool Exito, ErrorProducto Error = ErrorProducto.Ninguno, Producto? Producto = null)
    {
        public static ResultadoProducto Ok(Producto producto) => new(true, ErrorProducto.Ninguno, producto);
        public static ResultadoProducto Falla(ErrorProducto error) => new(false, error, null);
    }

    // Datos de un producto para crear/editar. Las fotos son rutas ya guardadas en disco (o null).
    // El SKU no viene aqui: se autogenera al crear y no se modifica al editar.
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

    // Una opcion del <select> de proveedores.
    public sealed record ProveedorOpcion(int Id, string Nombre);

    // Datos auxiliares para armar el formulario: proveedores y sugerencias (valores ya usados).
    public sealed record ProductoFormData(
        IReadOnlyList<ProveedorOpcion> Proveedores,
        IReadOnlyList<string> Categorias,
        IReadOnlyList<string> Epocas,
        IReadOnlyList<string> Estados);

    // Contrato del modulo de Inventario/Productos (Admin/Gerente). Concentra: autogeneracion de SKU,
    // sincronia disponibilidad=(stock>0), validacion de proveedor y guarda de borrado (FK ventas).
    public interface IProductoService
    {
        // Todos los productos para la lista de inventario (con proveedor, miniatura y si es borrable).
        Task<IReadOnlyList<ProductoListItemViewModel>> ListarAsync();

        // Productos para el catalogo (vista del vendedor): disponibles primero, con sus fotos.
        Task<IReadOnlyList<ProductoCatalogoViewModel>> ListarCatalogoAsync();

        // Ficha de detalle de un producto para el catalogo (con proveedor y las 3 fotos). Null si no existe.
        Task<ProductoDetalleViewModel?> ObtenerDetalleAsync(int id);

        // Busqueda del catalogo (buscador en vivo): filtra por texto libre y por categoria/epoca/estado.
        Task<IReadOnlyList<ProductoCatalogoViewModel>> BuscarCatalogoAsync(string? texto, string? categoria, string? epoca, string? estado);

        // Un producto por id (null si no existe).
        Task<Producto?> ObtenerAsync(int id);

        // Proveedores y sugerencias para los formularios de crear/editar.
        Task<ProductoFormData> ObtenerDatosFormularioAsync();

        // Crea el producto con SKU autogenerado y disponibilidad sincronizada. Falla si el proveedor
        // no existe.
        Task<ResultadoProducto> CrearAsync(ProductoDatos datos);

        // Actualiza el producto y resincroniza la disponibilidad. Falla si no existe, el proveedor no
        // es valido, o hay conflicto de concurrencia.
        Task<ResultadoProducto> EditarAsync(int id, ProductoDatos datos);

        // Elimina el producto (y sus fotos del disco). Falla si tiene ventas registradas (FK Restrict).
        Task<ResultadoProducto> EliminarAsync(int id);
    }
}
