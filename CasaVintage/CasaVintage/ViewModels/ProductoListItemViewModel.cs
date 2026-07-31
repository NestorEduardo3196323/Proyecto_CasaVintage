namespace CasaVintage.ViewModels
{
    // Fila de la lista de inventario. Incluye la primera foto (miniatura), el nombre del proveedor
    // y si el producto se puede eliminar (no se puede si ya tiene ventas registradas).
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
