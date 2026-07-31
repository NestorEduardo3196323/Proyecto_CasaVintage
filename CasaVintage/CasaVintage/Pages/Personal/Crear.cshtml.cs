using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Personal
{
    // Alta de una cuenta del personal. Solo el Administrador. La PageModel orquesta: valida la
    // entrada, delega en el service y traduce el motivo de falla a un mensaje en el campo correcto.
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

        // Opciones del <select> de rol.
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

            // Se guarda la foto (si la hay) antes de crear la cuenta; si el formato o tamano fallan,
            // se muestra el error sin tocar la base.
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
                // El alta fallo: se borra la imagen recien guardada para no dejar archivos huerfanos.
                _archivos.Eliminar(fotoRuta);
                AplicarError(resultado.Error);
                return Page();
            }

            TempData["MensajePersonal"] = $"Cuenta creada para {resultado.Usuario!.NombreUsuario}.";
            return RedirectToPage("Index");
        }

        private void AplicarError(ErrorUsuario error)
        {
            switch (error)
            {
                case ErrorUsuario.CorreoDuplicado:
                    ModelState.AddModelError("Entrada.Correo", "Ya existe una cuenta con ese correo.");
                    break;
                case ErrorUsuario.RolInvalido:
                    ModelState.AddModelError("Entrada.Rol", "Selecciona un rol valido.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta. Intenta de nuevo.");
                    break;
            }
        }

        // Traduce el error de la imagen a un mensaje para el usuario. Compartido con la edicion.
        internal static string MensajeFoto(ErrorArchivo error) => error switch
        {
            ErrorArchivo.FormatoNoValido => "La foto debe ser una imagen JPG, PNG o WEBP.",
            ErrorArchivo.DemasiadoGrande => "La foto no puede superar los 8 MB.",
            _ => "No se pudo guardar la foto. Intenta con otra imagen."
        };
    }
}
