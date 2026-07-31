using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos que captura la pantalla de login. Las Data Annotations dan validacion en cliente
    // (jQuery unobtrusive) y en servidor (ModelState). No hay "recordarme" ni registro publico.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;
    }
}
