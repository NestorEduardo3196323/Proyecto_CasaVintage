namespace CasaVintage.ViewModels
{
    // Proyeccion de un producto para las tarjetas del catalogo (lo que ve el vendedor al vender).
    // Incluye las 3 fotos (para el hover que se agrega despues) y los datos de la tarjeta.
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
