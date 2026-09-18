namespace CasaVintage.Models
{
    // Entity representing the counter customer captured on each sale. Maps to "clientes".
    // Business rule: repeated customers are allowed (no dedupe by email).
    public class Cliente
    {
        public int IdCliente { get; set; }

        // Customer name for the invoice (required).
        public string Nombre { get; set; } = string.Empty;

        // Customer phone (optional).
        public string? Telefono { get; set; }

        // Customer email to send the receipt (optional).
        public string? Correo { get; set; }

        // Sales associated with this customer.
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
