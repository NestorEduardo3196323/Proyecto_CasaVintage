using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data to create or edit a supplier (Admin/Manager). The Data Annotations validate on the
    // client and the server. Contact and phone are optional (nullable columns in the schema).
    public class ProveedorFormViewModel
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(60, ErrorMessage = "The name cannot exceed 60 characters.")]
        [Display(Name = "Name")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "The contact cannot exceed 100 characters.")]
        [Display(Name = "Contact")]
        public string? Contacto { get; set; }

        [StringLength(15, ErrorMessage = "The phone cannot exceed 15 characters.")]
        [Display(Name = "Phone")]
        public string? Telefono { get; set; }
    }
}
