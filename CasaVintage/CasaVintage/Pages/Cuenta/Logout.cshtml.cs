using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Cuenta
{
    // Cierre de sesion. Solo por POST (con anti-forgery) para no cerrar sesion por un simple GET.
    // Borra la cookie de autenticacion y devuelve al login.
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Un GET a /Cuenta/Logout no cierra sesion; redirige al login.
            return RedirectToPage("/Cuenta/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Cuenta/Login");
        }
    }
}
