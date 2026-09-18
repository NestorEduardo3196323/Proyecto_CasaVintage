namespace CasaVintage.ViewModels
{
    // Projection of a product for its detail card in the catalog. Includes the full description,
    // the supplier and the 3 photos (for the manual gallery with arrows/thumbnails).
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
