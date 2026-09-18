namespace CasaVintage.Models
{
    // Entity representing a store supplier. Maps to the "proveedores" table.
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        // Commercial name of the supplier (required).
        public string Nombre { get; set; } = string.Empty;

        // Contact person or channel (optional).
        public string? Contacto { get; set; }

        // Contact phone (optional).
        public string? Telefono { get; set; }

        // Products supplied by this supplier.
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
