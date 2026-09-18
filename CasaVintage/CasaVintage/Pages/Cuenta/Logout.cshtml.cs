using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Cuenta
{
    // Sign-out. Only via POST (with anti-forgery) so a plain GET does not end the session.
    // Clears the authentication cookie and returns to the login.
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // A GET to /Cuenta/Logout does not sign out; it redirects to the login.
            return RedirectToPage("/Cuenta/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Cuenta/Login");
        }
    }
}
