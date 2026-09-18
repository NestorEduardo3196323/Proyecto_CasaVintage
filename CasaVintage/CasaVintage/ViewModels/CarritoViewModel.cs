namespace CasaVintage.ViewModels
{
    // A cart line, with the current product data to display and charge it.
    public sealed record CarritoItemViewModel(
        int IdProducto,
        string Sku,
        string Nombre,
        string? Foto,
        decimal PrecioUnitario,
        int Cantidad,
        int StockDisponible)
    {
        // Total of this line (price x quantity).
        public decimal Subtotal => PrecioUnitario * Cantidad;

        // True if the requested quantity exceeds the available stock (warning to the salesperson).
        public bool ExcedeStock => Cantidad > StockDisponible;
    }

    // The whole cart: its lines and the totals.
    public sealed record CarritoViewModel(IReadOnlyList<CarritoItemViewModel> Items)
    {
        // Sum of the quantities (for the cart icon counter).
        public int TotalProductos => Items.Sum(i => i.Cantidad);

        // Sum of the subtotals (total to charge).
        public decimal TotalPrecio => Items.Sum(i => i.Subtotal);

        public bool Vacio => Items.Count == 0;

        // True if any line exceeds the stock (blocks the checkout until fixed).
        public bool HayExcesoStock => Items.Any(i => i.ExcedeStock);
    }
}
