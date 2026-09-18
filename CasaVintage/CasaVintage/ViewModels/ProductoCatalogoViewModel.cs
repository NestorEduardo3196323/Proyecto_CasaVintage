namespace CasaVintage.ViewModels
{
    // Projection of a product for the catalog cards (what the salesperson sees when selling).
    // Includes the 3 photos (for the hover) and the card data.
    public sealed record ProductoCatalogoViewModel(
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
        string? Foto1,
        string? Foto2,
        string? Foto3);
}
