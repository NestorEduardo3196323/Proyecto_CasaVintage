namespace CasaVintage.Models
{
    // Entidad que representa a un proveedor de la tienda. Mapea a la tabla "proveedores".
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        // Nombre comercial del proveedor (obligatorio).
        public string Nombre { get; set; } = string.Empty;

        // Persona o medio de contacto (opcional).
        public string? Contacto { get; set; }

        // Telefono de contacto (opcional).
        public string? Telefono { get; set; }

        // Productos suministrados por este proveedor.
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
