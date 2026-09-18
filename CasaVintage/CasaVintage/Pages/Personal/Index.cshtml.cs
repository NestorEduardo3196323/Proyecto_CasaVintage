using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // "Company staff" view: the team showcase and the account-management hub. Administrator only.
    // The PageModel orchestrates: it builds the cards (never passes the raw entity), and exposes
    // handlers to change the status (enable/disable) and reset the password. All handlers use the
    // POST-redirect-GET pattern so the action is not repeated on refresh.
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly IUsuarioService _usuarios;

        public IndexModel(IUsuarioService usuarios)
        {
            _usuarios = usuarios;
        }

        // Staff cards for the view.
        public IReadOnlyList<UsuarioCardViewModel> Personal { get; private set; } = Array.Empty<UsuarioCardViewModel>();

        // Data for the reset-password form (single reusable dialog).
        [BindProperty]
        public RestablecerPasswordViewModel Restablecer { get; set; } = new();

        // True when the reset failed validation: the view reopens the dialog with the error.
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
                    ? $"{resultado.Usuario!.NombreUsuario}'s account enabled."
                    : $"{resultado.Usuario!.NombreUsuario}'s account disabled.";
            }
            else
            {
                TempData["MensajePersonalError"] = MensajeError(resultado.Error);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestablecerAsync()
        {
            // Only the password fields are validated; if they fail, the dialog reopens with the error.
            if (!ModelState.IsValid)
            {
                await CargarAsync();
                ReabrirRestablecer = true;
                return Page();
            }

            var resultado = await _usuarios.RestablecerPasswordAsync(Restablecer.IdUsuario, Restablecer.Password);

            if (resultado.Exito)
            {
                TempData["MensajePersonal"] = $"Password reset for {resultado.Usuario!.NombreUsuario}.";
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

        // Id of the signed-in administrator (for the "cannot change your own status" guards).
        private int IdUsuarioActual()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }

        private static string MensajeError(ErrorUsuario error) => error switch
        {
            ErrorUsuario.NoPuedeCambiarPropioEstado => "You cannot change the status of your own account.",
            ErrorUsuario.UltimoAdministrador => "Cannot do this: it is the only active Administrator in the system.",
            ErrorUsuario.NoEncontrado => "The account no longer exists.",
            _ => "The action could not be completed. Try again."
        };
    }
}
