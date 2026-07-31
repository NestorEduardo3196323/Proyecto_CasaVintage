using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Cuenta
{
    // Pagina que se muestra cuando un usuario autenticado intenta entrar a una seccion que su
    // rol no permite (lo redirige aqui el AccessDeniedPath de la cookie).
    [AllowAnonymous]
    public class DenegadoModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
