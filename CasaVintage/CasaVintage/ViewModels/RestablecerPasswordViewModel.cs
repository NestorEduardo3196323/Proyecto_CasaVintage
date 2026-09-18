using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data to reset an account's password (Administrator only). The administrator types the new
    // password; the service hashes it. The previous password is never shown nor recoverable
    // (it is hashed).
    public class RestablecerPasswordViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "The password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "The password must include at least one letter and one number.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm the password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
