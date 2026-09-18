namespace CasaVintage.ViewModels
{
    // Inventory list row. Includes the first photo (thumbnail), the supplier name and whether the
    // product can be deleted (it cannot if it already has registered sales).
    public sealed record ProductoListItemViewModel(
        int IdProducto,
        string Sku,
        string Nombre,
        string Categoria,
        string Epoca,
        decimal Precio,
        decimal Costo,
        int Stock,
        int StockMinimo,
        bool Disponibilidad,
        string ProveedorNombre,
        string? Foto,
        bool PuedeEliminar);
}
