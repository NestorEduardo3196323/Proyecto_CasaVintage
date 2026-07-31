namespace CasaVintage.ViewModels
{
    // Una linea de la venta (para la confirmacion / comprobante).
    public sealed record VentaLineaViewModel(
        string Sku,
        string Nombre,
        int Cantidad,
        decimal PrecioUnitario)
    {
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }

    // Resumen de una venta ya registrada, para la pantalla de confirmacion.
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
        // Texto del metodo de pago para mostrar (incluye los ultimos 4 digitos si fue con tarjeta).
        public string MetodoPagoTexto => MetodoPago == "Tarjeta" && !string.IsNullOrEmpty(TarjetaUltimos4)
            ? $"Tarjeta terminada en {TarjetaUltimos4}"
            : MetodoPago;
    }
}
