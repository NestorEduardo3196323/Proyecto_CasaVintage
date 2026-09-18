using CasaVintage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Ajustes
{
    // Company visual identity: the Administrator uploads or removes the logo shown in the app.
    // Only the Administrator (owner) manages the brand. The PageModel orchestrates and delegates to
    // IMarcaService.
    [Authorize(Roles = "Administrador")]
    public class MarcaModel : PageModel
    {
        private readonly IMarcaService _marca;

        public MarcaModel(IMarcaService marca)
        {
            _marca = marca;
        }

        // Web path of the current logo (null if no logo has been uploaded yet).
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

            Mensaje = "Logo updated.";
            return RedirectToPage();
        }

        public IActionResult OnPostQuitar()
        {
            _marca.QuitarLogo();
            Mensaje = "The logo was removed. The app uses the default monogram.";
            return RedirectToPage();
        }
    }
}
