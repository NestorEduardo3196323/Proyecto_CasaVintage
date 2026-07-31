namespace CasaVintage.Models
{
    // Entidad que representa al cliente de mostrador capturado en cada venta. Mapea a "clientes".
    // Regla de negocio: se permiten clientes repetidos (no se dedupea por correo).
    public class Cliente
    {
        public int IdCliente { get; set; }

        // Nombre del cliente para la factura (obligatorio).
        public string Nombre { get; set; } = string.Empty;

        // Telefono del cliente (opcional).
        public string? Telefono { get; set; }

        // Correo del cliente para enviar el comprobante (opcional).
        public string? Correo { get; set; }

        // Ventas asociadas a este cliente.
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
