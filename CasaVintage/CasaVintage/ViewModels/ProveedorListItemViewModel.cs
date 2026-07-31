namespace CasaVintage.ViewModels
{
    // Fila de la tabla de proveedores. Incluye el numero de productos suministrados: sirve de
    // informacion y explica por que un proveedor con productos no se puede eliminar.
    public sealed record ProveedorListItemViewModel(
        int IdProveedor,
        string Nombre,
        string? Contacto,
        string? Telefono,
        int CantidadProductos);
}
