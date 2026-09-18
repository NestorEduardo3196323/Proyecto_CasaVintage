using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Cuenta
{
    // Page shown when an authenticated user tries to enter a section their role does not allow
    // (the cookie's AccessDeniedPath redirects here).
    [AllowAnonymous]
    public class DenegadoModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
