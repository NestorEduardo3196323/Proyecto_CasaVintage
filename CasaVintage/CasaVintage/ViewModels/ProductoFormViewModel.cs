using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data to create or edit an inventory product (Admin/Manager). The SKU is not captured:
    // it is auto-generated on create and not edited. Up to 3 photos (optional) saved to disk.
    public class ProductoFormViewModel
    {
        public int IdProducto { get; set; }

        // Read-only in the view (auto-generated). Shown when editing.
        public string? Sku { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
        [Display(Name = "Name")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "The description is required.")]
        [Display(Name = "Description")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "The era is required.")]
        [StringLength(50, ErrorMessage = "The era cannot exceed 50 characters.")]
        [Display(Name = "Era")]
        public string Epoca { get; set; } = string.Empty;

        [Required(ErrorMessage = "The condition is required.")]
        [StringLength(50, ErrorMessage = "The condition cannot exceed 50 characters.")]
        [Display(Name = "Condition")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "The category is required.")]
        [StringLength(50, ErrorMessage = "The category cannot exceed 50 characters.")]
        [Display(Name = "Category")]
        public string Categoria { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "The price must be a valid value (0 or more).")]
        [Display(Name = "Sale price")]
        public decimal Precio { get; set; }

        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "The cost must be a valid value (0 or more).")]
        [Display(Name = "Acquisition cost")]
        public decimal Costo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The stock cannot be negative.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The minimum stock cannot be negative.")]
        [Display(Name = "Minimum stock")]
        public int StockMinimo { get; set; }

        [Required(ErrorMessage = "Select a supplier.")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a supplier.")]
        [Display(Name = "Supplier")]
        public int IdProveedor { get; set; }

        // New photos (optional). When editing, they replace the current one in that slot.
        [Display(Name = "Photo 1")]
        public IFormFile? Foto1 { get; set; }

        [Display(Name = "Photo 2")]
        public IFormFile? Foto2 { get; set; }

        [Display(Name = "Photo 3")]
        public IFormFile? Foto3 { get; set; }

        // Paths of the current photos (only to show them when editing).
        public string? Foto1Actual { get; set; }
        public string? Foto2Actual { get; set; }
        public string? Foto3Actual { get; set; }

        // Check to remove the current photo in that slot (when editing).
        public bool QuitarFoto1 { get; set; }
        public bool QuitarFoto2 { get; set; }
        public bool QuitarFoto3 { get; set; }
    }
}
