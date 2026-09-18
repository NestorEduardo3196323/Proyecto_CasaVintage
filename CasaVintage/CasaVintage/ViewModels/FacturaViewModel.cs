namespace CasaVintage.ViewModels
{
    // Complete data for the receipt (internal invoice) of a sale: company, customer, lines and the
    // VAT breakdown. Sale prices are considered VAT-inclusive (13%, El Salvador), so the subtotal
    // and the VAT are computed from the total, without changing what the customer paid.
    public sealed record FacturaViewModel(
        int NumeroVenta,
        DateTime Fecha,
        string EmpresaNombre,
        string EmpresaDireccion,
        string EmpresaTelefono,
        string EmpresaCorreo,
        string ClienteNombre,
        string? ClienteCorreo,
        string MetodoPago,
        string VendedorNombre,
        IReadOnlyList<VentaLineaViewModel> Lineas,
        decimal Subtotal,
        decimal Iva,
        decimal Total,
        string? TarjetaUltimos4 = null)
    {
        // VAT rate used for the breakdown (13%).
        public const decimal TasaIva = 0.13m;

        // Payment method for display (includes the last 4 digits if it was by card).
        public string MetodoPagoTexto => MetodoPago switch
        {
            "Tarjeta" => !string.IsNullOrEmpty(TarjetaUltimos4) ? $"Card ending in {TarjetaUltimos4}" : "Card",
            "Efectivo" => "Cash",
            _ => MetodoPago
        };
    }
}
