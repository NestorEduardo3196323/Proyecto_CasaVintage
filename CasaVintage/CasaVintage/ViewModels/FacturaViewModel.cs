namespace CasaVintage.ViewModels
{
    // Datos completos del comprobante (factura interna) de una venta: empresa, cliente, lineas y el
    // desglose de IVA. Los precios de venta se consideran con IVA incluido (13%, El Salvador), asi
    // que el subtotal y el IVA se calculan a partir del total, sin cambiar lo que pago el cliente.
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
        // Tasa de IVA usada para el desglose (13%).
        public const decimal TasaIva = 0.13m;

        // Metodo de pago para mostrar (incluye los ultimos 4 digitos si fue con tarjeta).
        public string MetodoPagoTexto => MetodoPago == "Tarjeta" && !string.IsNullOrEmpty(TarjetaUltimos4)
            ? $"Tarjeta terminada en {TarjetaUltimos4}"
            : MetodoPago;
    }
}
