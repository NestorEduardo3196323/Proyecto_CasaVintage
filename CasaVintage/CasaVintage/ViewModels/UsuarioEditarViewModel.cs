using System.ComponentModel.DataAnnotations;

namespace CasaVintage.ViewModels
{
    // Datos para editar una cuenta del personal (solo Administrador). No incluye contrasena
    // (eso va por "Restablecer contrasena") ni el estado activo (eso va por el boton
    // activar/desactivar), para que cada accion sensible tenga un unico camino y sus guardas.
    public class UsuarioEditarViewModel
    {
        public int IdUsuario { get; set; }

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

        // Ruta de la foto actual (solo para mostrarla; no se edita directamente).
        public string? FotoActual { get; set; }

        // Nueva foto de perfil (opcional). Si se sube, reemplaza a la actual.
        [Display(Name = "Cambiar foto de perfil")]
        public IFormFile? Foto { get; set; }

        // Si se marca, se quita la foto actual (vuelve al avatar de iniciales).
        [Display(Name = "Quitar la foto actual")]
        public bool QuitarFoto { get; set; }
    }
}
