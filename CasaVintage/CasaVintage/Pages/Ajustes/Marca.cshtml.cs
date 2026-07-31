using CasaVintage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Ajustes
{
    // Identidad visual de la empresa: el Administrador sube o quita el logo que se muestra en la app.
    // Solo el Administrador (dueno) gestiona la marca. La PageModel orquesta y delega en IMarcaService.
    [Authorize(Roles = "Administrador")]
    public class MarcaModel : PageModel
    {
        private readonly IMarcaService _marca;

        public MarcaModel(IMarcaService marca)
        {
            _marca = marca;
        }

        // Ruta web del logo actual (null si aun no hay logo cargado).
        public string? LogoActual { get; private set; }

        [BindProperty]
        public IFormFile? Logo { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        public void OnGet()
        {
            LogoActual = _marca.RutaLogo();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            var error = await _marca.GuardarLogoAsync(Logo);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                LogoActual = _marca.RutaLogo();
                return Page();
            }

            Mensaje = "Logo actualizado.";
            return RedirectToPage();
        }

        public IActionResult OnPostQuitar()
        {
            _marca.QuitarLogo();
            Mensaje = "Se quito el logo. La app usa el monograma por defecto.";
            return RedirectToPage();
        }
    }
}
