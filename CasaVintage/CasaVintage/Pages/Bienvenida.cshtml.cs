using System.Security.Claims;
using CasaVintage.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages
{
    // Aterrizaje provisional para roles cuya seccion de trabajo aun no se construye. No es un
    // dashboard de modulos: solo un saludo. En cuanto el rol tenga una seccion habilitada,
    // RolRutas.LandingPara lo lleva directo ahi y esta pagina deja de usarse para ese rol.
    public class BienvenidaModel : PageModel
    {
        // Rol del usuario en sesion (para personalizar el mensaje).
        public string? Rol { get; private set; }

        // True si el rol ya tiene alguna seccion disponible (por si llega aqui por escribir la URL).
        public bool TieneSeccion { get; private set; }

        public void OnGet()
        {
            Rol = User.FindFirstValue(ClaimTypes.Role);
            TieneSeccion = MenuRol.PrimeraDisponible(Rol) is not null;
        }
    }
}
