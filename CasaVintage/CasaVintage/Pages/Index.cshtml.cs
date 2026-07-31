using System.Security.Claims;
using CasaVintage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages
{
    // Pagina de entrada. Requiere autenticacion (convencion global) y reenvia a cada usuario a su
    // primera seccion disponible. Un anonimo nunca llega aqui: la cookie lo manda antes al login.
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage(RolRutas.LandingPara(User.FindFirstValue(ClaimTypes.Role)));
        }
    }
}
