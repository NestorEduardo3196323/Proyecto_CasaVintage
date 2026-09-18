using System.Security.Claims;
using CasaVintage.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages
{
    // Provisional landing for roles whose work section is not built yet. It is not a module
    // dashboard: just a greeting. As soon as the role has an enabled section, RolRutas.LandingPara
    // takes it straight there and this page is no longer used for that role.
    public class BienvenidaModel : PageModel
    {
        // Role of the signed-in user (to personalize the message).
        public string? Rol { get; private set; }

        // True if the role already has any available section (in case it arrives here by typing the URL).
        public bool TieneSeccion { get; private set; }

        public void OnGet()
        {
            Rol = User.FindFirstValue(ClaimTypes.Role);
            TieneSeccion = MenuRol.PrimeraDisponible(Rol) is not null;
        }
    }
}
