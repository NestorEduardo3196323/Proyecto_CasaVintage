using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // Edicion de los datos de una cuenta (nombre, correo, rol). Solo el Administrador. La contrasena
    // y el estado activo no se tocan aqui: cada uno tiene su propia accion con sus guardas.
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

            // Se decide el cambio de foto: quitar, reemplazar por una nueva, o dejar la actual.
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
                // La edicion fallo: se borra la imagen recien subida para no dejar archivos huerfanos.
                if (cambiarFoto)
                {
                    _archivos.Eliminar(nuevaFoto);
                }
                AplicarError(resultado.Error);
                await RecargarFotoActualAsync();
                return Page();
            }

            TempData["MensajePersonal"] = $"Cuenta de {resultado.Usuario!.NombreUsuario} actualizada.";
            return RedirectToPage("Index");
        }

        // Vuelve a leer la foto actual desde la base para mostrarla al re-renderizar tras un error.
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
                    ModelState.AddModelError("Entrada.Correo", "Ya existe otra cuenta con ese correo.");
                    break;
                case ErrorUsuario.RolInvalido:
                    ModelState.AddModelError("Entrada.Rol", "Selecciona un rol valido.");
                    break;
                case ErrorUsuario.UltimoAdministrador:
                    ModelState.AddModelError(string.Empty, "No puedes quitar el rol de Administrador: es el unico administrador activo del sistema.");
                    break;
                case ErrorUsuario.NoEncontrado:
                    ModelState.AddModelError(string.Empty, "La cuenta ya no existe.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar la cuenta. Intenta de nuevo.");
                    break;
            }
        }
    }
}
