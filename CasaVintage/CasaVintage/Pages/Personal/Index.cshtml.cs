using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // Vista "Personal de la empresa": el showcase del equipo y el centro de gestion de cuentas.
    // Solo el Administrador. La PageModel orquesta: arma las tarjetas (nunca pasa la entidad cruda),
    // y expone handlers para cambiar el estado (activar/desactivar) y restablecer la contrasena.
    // Todos los handlers usan patron POST-redirect-GET para no repetir la accion al refrescar.
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly IUsuarioService _usuarios;

        public IndexModel(IUsuarioService usuarios)
        {
            _usuarios = usuarios;
        }

        // Tarjetas del personal para la vista.
        public IReadOnlyList<UsuarioCardViewModel> Personal { get; private set; } = Array.Empty<UsuarioCardViewModel>();

        // Datos del formulario de restablecer contrasena (dialogo unico reutilizable).
        [BindProperty]
        public RestablecerPasswordViewModel Restablecer { get; set; } = new();

        // True cuando el restablecimiento fallo la validacion: la vista reabre el dialogo con el error.
        public bool ReabrirRestablecer { get; private set; }

        public async Task OnGetAsync()
        {
            await CargarAsync();
        }

        public async Task<IActionResult> OnPostCambiarEstadoAsync(int id, bool activar)
        {
            var resultado = await _usuarios.CambiarEstadoAsync(id, activar, IdUsuarioActual());

            if (resultado.Exito)
            {
                TempData["MensajePersonal"] = activar
                    ? $"Cuenta de {resultado.Usuario!.NombreUsuario} activada."
                    : $"Cuenta de {resultado.Usuario!.NombreUsuario} desactivada.";
            }
            else
            {
                TempData["MensajePersonalError"] = MensajeError(resultado.Error);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestablecerAsync()
        {
            // Solo se validan los campos de la contrasena; si fallan, se reabre el dialogo con el error.
            if (!ModelState.IsValid)
            {
                await CargarAsync();
                ReabrirRestablecer = true;
                return Page();
            }

            var resultado = await _usuarios.RestablecerPasswordAsync(Restablecer.IdUsuario, Restablecer.Password);

            if (resultado.Exito)
            {
                TempData["MensajePersonal"] = $"Contrasena restablecida para {resultado.Usuario!.NombreUsuario}.";
            }
            else
            {
                TempData["MensajePersonalError"] = MensajeError(resultado.Error);
            }

            return RedirectToPage();
        }

        private async Task CargarAsync()
        {
            var idActual = IdUsuarioActual();
            var usuarios = await _usuarios.ListarAsync();

            Personal = usuarios.Select(u => new UsuarioCardViewModel(
                u.IdUsuario,
                u.NombreUsuario,
                u.Correo,
                u.Rol,
                RolInfo.Descripcion(u.Rol),
                u.Activo,
                u.FechaCreado,
                u.IdUsuario == idActual,
                u.Foto)).ToList();
        }

        // Id del administrador en sesion (para las guardas de "no cambiar tu propio estado").
        private int IdUsuarioActual()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }

        private static string MensajeError(ErrorUsuario error) => error switch
        {
            ErrorUsuario.NoPuedeCambiarPropioEstado => "No puedes cambiar el estado de tu propia cuenta.",
            ErrorUsuario.UltimoAdministrador => "No se puede: es el unico Administrador activo del sistema.",
            ErrorUsuario.NoEncontrado => "La cuenta ya no existe.",
            _ => "No se pudo completar la accion. Intenta de nuevo."
        };
    }
}
