using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos para crear o editar un proveedor (Admin/Gerente). Las Data Annotations validan en
    // cliente y servidor. Contacto y telefono son opcionales (columnas nullable en el esquema).
    public class ProveedorFormViewModel
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El contacto no puede superar 100 caracteres.")]
        [Display(Name = "Contacto")]
        public string? Contacto { get; set; }

        [StringLength(15, ErrorMessage = "El telefono no puede superar 15 caracteres.")]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }
    }
}
