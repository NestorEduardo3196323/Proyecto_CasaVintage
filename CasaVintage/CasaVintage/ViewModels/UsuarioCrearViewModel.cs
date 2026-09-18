using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data to create a staff account (Administrator only). The Data Annotations validate on the
    // client (jQuery unobtrusive) and the server (ModelState). The account is created active; the
    // status is changed later with the enable/disable button. The password is hashed in the service.
    public class UsuarioCrearViewModel
    {
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

        // Profile photo (optional). Type and size are validated on save; the disk path is stored.
        [Display(Name = "Profile photo")]
        public IFormFile? Foto { get; set; }

        [Required(ErrorMessage = "The password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "The password must include at least one letter and one number.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm the password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
