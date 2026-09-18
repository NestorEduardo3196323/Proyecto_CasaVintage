namespace CasaVintage.Models
{
    // Entity representing the header of a sale. Maps to "ventas".
    // Each sale is processed as a single transaction (see the Sales Services).
    public class Venta
    {
        public int IdVenta { get; set; }

        // Date and time of the sale.
        public DateTime Fecha { get; set; }

        // Customer being invoiced.
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Salesperson who registered the sale.
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // Total charged in the sale.
        public decimal TotalPagado { get; set; }

        // Payment method: Efectivo | Tarjeta (validated by a CHECK in the DB).
        public string MetodoPago { get; set; } = string.Empty;

        // Only the last 4 digits of the card (simulated card payment). The full number and the CVV
        // are never stored. Null when the payment was in cash.
        public string? TarjetaUltimos4 { get; set; }

        // Detail lines of the sale.
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
