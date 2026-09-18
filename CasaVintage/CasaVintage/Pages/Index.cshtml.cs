using System.Security.Claims;
using CasaVintage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages
{
    // Entry page. Requires authentication (global convention) and forwards each user to their
    // first available section. An anonymous user never reaches here: the cookie sends them to login.
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage(RolRutas.LandingPara(User.FindFirstValue(ClaimTypes.Role)));
        }
    }
}
