namespace CasaVintage.Models
{
    // Entidad que representa la cabecera de una venta. Mapea a "ventas".
    // Cada venta se procesa como una sola transaccion (ver Services de Ventas).
    public class Venta
    {
        public int IdVenta { get; set; }

        // Fecha y hora de la venta.
        public DateTime Fecha { get; set; }

        // Cliente al que se le factura.
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Vendedor que registro la venta.
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // Total cobrado en la venta.
        public decimal TotalPagado { get; set; }

        // Metodo de pago: Efectivo | Tarjeta (validado por CHECK en la BD).
        public string MetodoPago { get; set; } = string.Empty;

        // Solo los ultimos 4 digitos de la tarjeta (pago con tarjeta simulado). Nunca se guarda el
        // numero completo ni el CVV. Null cuando el pago fue en efectivo.
        public string? TarjetaUltimos4 { get; set; }

        // Lineas de detalle de la venta.
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
