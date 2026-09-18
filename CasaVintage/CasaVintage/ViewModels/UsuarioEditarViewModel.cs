using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data to edit a staff account (Administrator only). It does not include the password (that goes
    // through "Reset password") nor the active status (that goes through the enable/disable button),
    // so each sensitive action has a single path and its guards.
    public class UsuarioEditarViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(60, ErrorMessage = "The name cannot exceed 60 characters.")]
        [Display(Name = "Full name")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "The email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [StringLength(150, ErrorMessage = "The email cannot exceed 150 characters.")]
        [Display(Name = "Email")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select a role.")]
        [Display(Name = "Role")]
        public string Rol { get; set; } = string.Empty;

        // Path of the current photo (only to show it; not edited directly).
        public string? FotoActual { get; set; }

        // New profile photo (optional). If uploaded, it replaces the current one.
        [Display(Name = "Change profile photo")]
        public IFormFile? Foto { get; set; }

        // If checked, the current photo is removed (goes back to the initials avatar).
        [Display(Name = "Remove the current photo")]
        public bool QuitarFoto { get; set; }
    }
}
