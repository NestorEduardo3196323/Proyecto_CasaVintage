namespace CasaVintage.Models
{
    // Entity representing an inventory item. Maps to "productos".
    // Rules: unique auto-generated SKU; up to 3 photos (paths on disk); availability = (stock > 0);
    // row_version for optimistic concurrency when selling.
    public class Producto
    {
        public int IdProducto { get; set; }

        // Unique code auto-generated when registering the product (RF-03).
        public string Sku { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        // Long description (varchar(max)).
        public string Descripcion { get; set; } = string.Empty;

        // Era of the item (e.g. Victorian, Art Deco).
        public string Epoca { get; set; } = string.Empty;

        // Conservation condition.
        public string Estado { get; set; } = string.Empty;

        // Sale price.
        public decimal Precio { get; set; }

        // Acquisition cost; used to compute profitability.
        public decimal Costo { get; set; }

        // Available stock.
        public int Stock { get; set; }

        // Desired minimum level (for the "low stock" alert). 0 = no alert. NEW.
        public int StockMinimo { get; set; }

        // Synced with the stock: availability = (stock > 0).
        public bool Disponibilidad { get; set; }

        public string Categoria { get; set; } = string.Empty;

        // Product supplier.
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Paths of up to 3 photos on disk (not the binary).
        public string? Foto1 { get; set; }
        public string? Foto2 { get; set; }
        public string? Foto3 { get; set; }

        // Registration date (audit).
        public DateTime FechaRegistro { get; set; }

        // Optimistic concurrency token (rowversion).
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Sale lines that include this product.
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
