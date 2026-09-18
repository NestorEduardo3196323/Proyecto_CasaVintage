namespace CasaVintage.Models
{
    // Entity representing a line of a sale (a product and its quantity). Maps to "detalle_venta".
    public class DetalleVenta
    {
        public int IdDetalle { get; set; }

        // Sale the line belongs to.
        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        // Sold product.
        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        // Quantity sold (greater than 0, validated by a CHECK in the DB).
        public int Cantidad { get; set; }

        // Unit price at the moment of the sale.
        public decimal PrecioUnitario { get; set; }
    }
}
