using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Data captured by the login screen. The Data Annotations provide client-side validation
    // (jQuery unobtrusive) and server-side validation (ModelState). There is no "remember me" nor
    // public sign-up.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [Display(Name = "Email")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}
