namespace CasaVintage.ViewModels
{
    // Una linea del carrito, con los datos actuales del producto para mostrarla y cobrar.
    public sealed record CarritoItemViewModel(
        int IdProducto,
        string Sku,
        string Nombre,
        string? Foto,
        decimal PrecioUnitario,
        int Cantidad,
        int StockDisponible)
    {
        // Total de esta linea (precio x cantidad).
        public decimal Subtotal => PrecioUnitario * Cantidad;

        // True si la cantidad pedida supera el stock disponible (aviso al vendedor).
        public bool ExcedeStock => Cantidad > StockDisponible;
    }

    // El carrito completo: sus lineas y los totales.
    public sealed record CarritoViewModel(IReadOnlyList<CarritoItemViewModel> Items)
    {
        // Suma de las cantidades (para el contador del icono del carrito).
        public int TotalProductos => Items.Sum(i => i.Cantidad);

        // Suma de los subtotales (total a cobrar).
        public decimal TotalPrecio => Items.Sum(i => i.Subtotal);

        public bool Vacio => Items.Count == 0;

        // True si alguna linea excede el stock (bloquea el cobro hasta corregir).
        public bool HayExcesoStock => Items.Any(i => i.ExcedeStock);
    }
}
