using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Cuenta
{
    // Pantalla de inicio de sesion. La PageModel solo orquesta: valida la entrada, delega la
    // verificacion al IAuthService, arma la cookie con los claims (incluido el rol) y redirige
    // al panel correspondiente. Accesible sin autenticar.
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Entrada { get; set; } = new();

        // A donde volver despues de iniciar sesion (cuando el usuario fue redirigido al login).
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // Si ya hay sesion activa, no mostrar el login: ir directo a su panel.
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage(RolRutas.LandingPara(User.FindFirstValue(ClaimTypes.Role)));
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _authService.ValidarCredencialesAsync(Entrada.Correo, Entrada.Password);

            if (!resultado.Exito)
            {
                // Cuenta desactivada: mensaje especifico. Credenciales malas: mensaje generico
                // (no se distingue correo inexistente de contrasena incorrecta).
                var mensaje = resultado.Estado == ResultadoAutenticacion.Inactivo
                    ? "Tu cuenta esta desactivada. Contacta al administrador."
                    : "Correo o contrasena incorrectos.";
                ModelState.AddModelError(string.Empty, mensaje);
                return Page();
            }

            var usuario = resultado.Usuario!;

            // Claims que viajan en la cookie; el rol habilita [Authorize(Roles=...)] en servidor.
            // FechaCreado se usa para la tarjeta de bienvenida (el esquema la tiene como fecha_creado).
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol),
                new(PerfilUsuario.ClaimFechaCreado, usuario.FechaCreado.ToString("o"))
            };

            // Foto de perfil (si tiene): permite mostrarla en el menu y la tarjeta de bienvenida.
            if (!string.IsNullOrEmpty(usuario.Foto))
            {
                claims.Add(new Claim(PerfilUsuario.ClaimFoto, usuario.Foto));
            }

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            // Marca para mostrar la tarjeta de bienvenida una sola vez, al aterrizar en el panel.
            TempData["MostrarBienvenida"] = "1";

            // Respeta el returnUrl solo si es local (evita redirecciones abiertas); si no, al panel del rol.
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage(RolRutas.LandingPara(usuario.Rol));
        }
    }
}
