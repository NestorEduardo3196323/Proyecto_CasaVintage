using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // Editing an account's data (name, email, role). Administrator only. The password and the active
    // status are not touched here: each one has its own action with its guards.
    [Authorize(Roles = "Administrador")]
    public class EditarModel : PageModel
    {
        private readonly IUsuarioService _usuarios;
        private readonly IAlmacenArchivos _archivos;

        public EditarModel(IUsuarioService usuarios, IAlmacenArchivos archivos)
        {
            _usuarios = usuarios;
            _archivos = archivos;
        }

        [BindProperty]
        public UsuarioEditarViewModel Entrada { get; set; } = new();

        public IReadOnlyList<string> Roles => RolInfo.Todos;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var usuario = await _usuarios.ObtenerAsync(id);
            if (usuario is null)
            {
                return NotFound();
            }

            Entrada = new UsuarioEditarViewModel
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                FotoActual = usuario.Foto
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await RecargarFotoActualAsync();
                return Page();
            }

            // The photo change is decided: remove, replace with a new one, or keep the current one.
            var cambiarFoto = false;
            string? nuevaFoto = null;

            if (Entrada.QuitarFoto)
            {
                cambiarFoto = true;
                nuevaFoto = null;
            }
            else if (Entrada.Foto is not null)
            {
                var guardado = await _archivos.GuardarImagenAsync(Entrada.Foto, "uploads/usuarios");
                if (!guardado.Exito)
                {
                    ModelState.AddModelError("Entrada.Foto", CrearModel.MensajeFoto(guardado.Error));
                    await RecargarFotoActualAsync();
                    return Page();
                }
                cambiarFoto = true;
                nuevaFoto = guardado.RutaWeb;
            }

            var resultado = await _usuarios.EditarAsync(
                Entrada.IdUsuario, Entrada.NombreUsuario, Entrada.Correo, Entrada.Rol, cambiarFoto, nuevaFoto);

            if (!resultado.Exito)
            {
                // The edit failed: the just-uploaded image is deleted so no orphan files are left.
                if (cambiarFoto)
                {
                    _archivos.Eliminar(nuevaFoto);
                }
                AplicarError(resultado.Error);
                await RecargarFotoActualAsync();
                return Page();
            }

            TempData["MensajePersonal"] = $"{resultado.Usuario!.NombreUsuario}'s account updated.";
            return RedirectToPage("Index");
        }

        // Re-reads the current photo from the database to show it when re-rendering after an error.
        private async Task RecargarFotoActualAsync()
        {
            var usuario = await _usuarios.ObtenerAsync(Entrada.IdUsuario);
            Entrada.FotoActual = usuario?.Foto;
        }

        private void AplicarError(ErrorUsuario error)
        {
            switch (error)
            {
                case ErrorUsuario.CorreoDuplicado:
                    ModelState.AddModelError("Entrada.Correo", "Another account with that email already exists.");
                    break;
                case ErrorUsuario.RolInvalido:
                    ModelState.AddModelError("Entrada.Rol", "Select a valid role.");
                    break;
                case ErrorUsuario.UltimoAdministrador:
                    ModelState.AddModelError(string.Empty, "You cannot remove the Administrator role: it is the only active administrator in the system.");
                    break;
                case ErrorUsuario.NoEncontrado:
                    ModelState.AddModelError(string.Empty, "The account no longer exists.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "The account could not be updated. Try again.");
                    break;
            }
        }
    }
}
