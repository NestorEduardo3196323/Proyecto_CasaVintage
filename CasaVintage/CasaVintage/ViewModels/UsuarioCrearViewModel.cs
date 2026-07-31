using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos para dar de alta una cuenta del personal (solo Administrador). Las Data Annotations
    // validan en cliente (jQuery unobtrusive) y en servidor (ModelState). La cuenta se crea activa;
    // el estado se cambia luego con el boton activar/desactivar. La contrasena se hashea en el service.
    public class UsuarioCrearViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;

        // Foto de perfil (opcional). Se valida tipo y tamano al guardar; se persiste la ruta en disco.
        [Display(Name = "Foto de perfil")]
        public IFormFile? Foto { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrasena debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contrasena debe incluir al menos una letra y un numero.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contrasena.")]
        [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contrasena")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
