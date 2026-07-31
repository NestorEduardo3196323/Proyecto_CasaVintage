namespace CasaVintage.Models
{
    // Entidad que representa una linea de una venta (un producto y su cantidad). Mapea a "detalle_venta".
    public class DetalleVenta
    {
        public int IdDetalle { get; set; }

        // Venta a la que pertenece la linea.
        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        // Producto vendido.
        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        // Cantidad vendida (mayor a 0, validado por CHECK en la BD).
        public int Cantidad { get; set; }

        // Precio unitario al momento de la venta.
        public decimal PrecioUnitario { get; set; }
    }
}
