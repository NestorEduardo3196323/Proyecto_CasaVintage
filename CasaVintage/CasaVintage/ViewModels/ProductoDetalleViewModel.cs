namespace CasaVintage.ViewModels
{
    // Proyeccion de un producto para su ficha de detalle en el catalogo. Incluye la descripcion
    // completa, el proveedor y las 3 fotos (para la galeria manual con flechas/miniaturas).
    public sealed record ProductoDetalleViewModel(
        int IdProducto,
        string Sku,
        string Nombre,
        string Descripcion,
        decimal Precio,
        string Epoca,
        string Estado,
        string Categoria,
        int Stock,
        bool Disponibilidad,
        string ProveedorNombre,
        string? Foto1,
        string? Foto2,
        string? Foto3);
}
