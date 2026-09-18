namespace CasaVintage.ViewModels
{
    // A line of the sale (for the confirmation / receipt).
    public sealed record VentaLineaViewModel(
        string Sku,
        string Nombre,
        int Cantidad,
        decimal PrecioUnitario)
    {
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }

    // Summary of an already-registered sale, for the confirmation screen.
    public sealed record VentaConfirmacionViewModel(
        int IdVenta,
        DateTime Fecha,
        string ClienteNombre,
        string? ClienteCorreo,
        string MetodoPago,
        decimal Total,
        string VendedorNombre,
        IReadOnlyList<VentaLineaViewModel> Lineas,
        string? TarjetaUltimos4 = null)
    {
        // Payment method text for display (includes the last 4 digits if it was by card).
        public string MetodoPagoTexto => MetodoPago switch
        {
            "Tarjeta" => !string.IsNullOrEmpty(TarjetaUltimos4) ? $"Card ending in {TarjetaUltimos4}" : "Card",
            "Efectivo" => "Cash",
            _ => MetodoPago
        };
    }
}
