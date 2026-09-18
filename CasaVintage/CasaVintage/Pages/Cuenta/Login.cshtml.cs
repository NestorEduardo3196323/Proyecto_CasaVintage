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
    // Sign-in screen. The PageModel only orchestrates: it validates the input, delegates the
    // verification to IAuthService, builds the cookie with the claims (including the role) and
    // redirects to the matching panel. Accessible without authentication.
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

        // Where to return after signing in (when the user was redirected to the login).
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // If there is already an active session, do not show the login: go straight to the panel.
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
                // Deactivated account: specific message. Bad credentials: generic message
                // (a non-existent email is not distinguished from a wrong password).
                var mensaje = resultado.Estado == ResultadoAutenticacion.Inactivo
                    ? "Your account is disabled. Contact the administrator."
                    : "Incorrect email or password.";
                ModelState.AddModelError(string.Empty, mensaje);
                return Page();
            }

            var usuario = resultado.Usuario!;

            // Claims that travel in the cookie; the role enables [Authorize(Roles=...)] on the server.
            // CreatedAt is used for the welcome card (the schema stores it as fecha_creado).
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol),
                new(PerfilUsuario.ClaimFechaCreado, usuario.FechaCreado.ToString("o"))
            };

            // Profile photo (if any): lets it show in the menu and the welcome card.
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

            // Flag to show the welcome card once, when landing on the panel.
            TempData["MostrarBienvenida"] = "1";

            // Respect the returnUrl only if it is local (avoids open redirects); otherwise, the role panel.
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage(RolRutas.LandingPara(usuario.Rol));
        }
    }
}
