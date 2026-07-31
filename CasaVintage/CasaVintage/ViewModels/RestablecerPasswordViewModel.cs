using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos para restablecer la contrasena de una cuenta (solo Administrador). El administrador
    // escribe la nueva contrasena; el service la hashea. La contrasena anterior nunca se muestra
    // ni se puede recuperar (esta hasheada).
    public class RestablecerPasswordViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrasena debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contrasena debe incluir al menos una letra y un numero.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contrasena")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contrasena.")]
        [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contrasena")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
