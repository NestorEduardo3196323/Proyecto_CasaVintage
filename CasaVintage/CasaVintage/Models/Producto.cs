namespace CasaVintage.Models
{
    // Entidad que representa un articulo del inventario. Mapea a "productos".
    // Reglas: SKU unico autogenerado; hasta 3 fotos (rutas en disco); disponibilidad = (stock > 0);
    // row_version para concurrencia optimista al vender.
    public class Producto
    {
        public int IdProducto { get; set; }

        // Codigo unico autogenerado al registrar el producto (RF-03).
        public string Sku { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        // Descripcion larga (varchar(max)).
        public string Descripcion { get; set; } = string.Empty;

        // Epoca del articulo (ej. Victoriana, Art Deco).
        public string Epoca { get; set; } = string.Empty;

        // Estado de conservacion.
        public string Estado { get; set; } = string.Empty;

        // Precio de venta.
        public decimal Precio { get; set; }

        // Costo de adquisicion; se usa para calcular rentabilidad.
        public decimal Costo { get; set; }

        // Existencias disponibles.
        public int Stock { get; set; }

        // Nivel minimo deseado (para la alerta de "stock bajo"). 0 = sin alerta. NUEVO.
        public int StockMinimo { get; set; }

        // Se sincroniza con el stock: disponibilidad = (stock > 0).
        public bool Disponibilidad { get; set; }

        public string Categoria { get; set; } = string.Empty;

        // Proveedor del producto.
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Rutas de hasta 3 fotos en disco (no el binario).
        public string? Foto1 { get; set; }
        public string? Foto2 { get; set; }
        public string? Foto3 { get; set; }

        // Fecha de registro (auditoria).
        public DateTime FechaRegistro { get; set; }

        // Token de concurrencia optimista (rowversion).
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Lineas de venta que incluyen este producto.
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
