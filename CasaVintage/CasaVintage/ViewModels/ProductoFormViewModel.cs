using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos para crear o editar un producto del inventario (Admin/Gerente). El SKU no se captura:
    // se autogenera al crear y no se edita. Hasta 3 fotos (opcionales) que se guardan en disco.
    public class ProductoFormViewModel
    {
        public int IdProducto { get; set; }

        // Solo lectura en la vista (autogenerado). Se muestra al editar.
        public string? Sku { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        [Display(Name = "Descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La epoca es obligatoria.")]
        [StringLength(50, ErrorMessage = "La epoca no puede superar 50 caracteres.")]
        [Display(Name = "Epoca")]
        public string Epoca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(50, ErrorMessage = "El estado no puede superar 50 caracteres.")]
        [Display(Name = "Estado de conservacion")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria es obligatoria.")]
        [StringLength(50, ErrorMessage = "La categoria no puede superar 50 caracteres.")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El precio debe ser un valor valido (0 o mas).")]
        [Display(Name = "Precio de venta")]
        public decimal Precio { get; set; }

        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El costo debe ser un valor valido (0 o mas).")]
        [Display(Name = "Costo de adquisicion")]
        public decimal Costo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock minimo no puede ser negativo.")]
        [Display(Name = "Stock minimo")]
        public int StockMinimo { get; set; }

        [Required(ErrorMessage = "Selecciona un proveedor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        // Fotos nuevas (opcionales). En edicion reemplazan a la actual de ese espacio.
        [Display(Name = "Foto 1")]
        public IFormFile? Foto1 { get; set; }

        [Display(Name = "Foto 2")]
        public IFormFile? Foto2 { get; set; }

        [Display(Name = "Foto 3")]
        public IFormFile? Foto3 { get; set; }

        // Rutas de las fotos actuales (solo para mostrarlas en edicion).
        public string? Foto1Actual { get; set; }
        public string? Foto2Actual { get; set; }
        public string? Foto3Actual { get; set; }

        // Marcar para quitar la foto actual de ese espacio (en edicion).
        public bool QuitarFoto1 { get; set; }
        public bool QuitarFoto2 { get; set; }
        public bool QuitarFoto3 { get; set; }
    }
}
