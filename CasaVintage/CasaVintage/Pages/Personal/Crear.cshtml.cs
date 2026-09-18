using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // Staff account creation. Administrator only. The PageModel orchestrates: it validates the input,
    // delegates to the service and translates the failure reason to a message on the correct field.
    [Authorize(Roles = "Administrador")]
    public class CrearModel : PageModel
    {
        private readonly IUsuarioService _usuarios;
        private readonly IAlmacenArchivos _archivos;

        public CrearModel(IUsuarioService usuarios, IAlmacenArchivos archivos)
        {
            _usuarios = usuarios;
            _archivos = archivos;
        }

        [BindProperty]
        public UsuarioCrearViewModel Entrada { get; set; } = new();

        // Options for the role <select>.
        public IReadOnlyList<string> Roles => RolInfo.Todos;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The photo (if any) is saved before creating the account; if the format or size fail,
            // the error is shown without touching the database.
            string? fotoRuta = null;
            if (Entrada.Foto is not null)
            {
                var guardado = await _archivos.GuardarImagenAsync(Entrada.Foto, "uploads/usuarios");
                if (!guardado.Exito)
                {
                    ModelState.AddModelError("Entrada.Foto", MensajeFoto(guardado.Error));
                    return Page();
                }
                fotoRuta = guardado.RutaWeb;
            }

            var resultado = await _usuarios.CrearAsync(
                Entrada.NombreUsuario, Entrada.Correo, Entrada.Rol, Entrada.Password, fotoRuta);

            if (!resultado.Exito)
            {
                // The creation failed: the just-saved image is deleted so no orphan files are left.
                _archivos.Eliminar(fotoRuta);
                AplicarError(resultado.Error);
                return Page();
            }

            TempData["MensajePersonal"] = $"Account created for {resultado.Usuario!.NombreUsuario}.";
            return RedirectToPage("Index");
        }

        private void AplicarError(ErrorUsuario error)
        {
            switch (error)
            {
                case ErrorUsuario.CorreoDuplicado:
                    ModelState.AddModelError("Entrada.Correo", "An account with that email already exists.");
                    break;
                case ErrorUsuario.RolInvalido:
                    ModelState.AddModelError("Entrada.Rol", "Select a valid role.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "The account could not be created. Try again.");
                    break;
            }
        }

        // Translates the image error to a message for the user. Shared with editing.
        internal static string MensajeFoto(ErrorArchivo error) => error switch
        {
            ErrorArchivo.FormatoNoValido => "The photo must be a JPG, PNG or WEBP image.",
            ErrorArchivo.DemasiadoGrande => "The photo cannot exceed 8 MB.",
            _ => "The photo could not be saved. Try another image."
        };
    }
}
