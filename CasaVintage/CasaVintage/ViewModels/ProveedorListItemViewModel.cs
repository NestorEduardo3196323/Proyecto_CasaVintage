namespace CasaVintage.ViewModels
{
    // Suppliers table row. Includes the number of products supplied: it is informative and explains
    // why a supplier with products cannot be deleted.
    public sealed record ProveedorListItemViewModel(
        int IdProveedor,
        string Nombre,
        string? Contacto,
        string? Telefono,
        int CantidadProductos);
}
