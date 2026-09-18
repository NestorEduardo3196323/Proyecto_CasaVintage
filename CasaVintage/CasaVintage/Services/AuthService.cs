using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Authentication service. Validates the email and password against the database using
    // IPasswordHasher (never compares plain text) and applies the business rule of blocking inactive
    // accounts. It does not handle the session cookie; the login PageModel takes care of that.
    public class AuthService : IAuthService
    {
        private readonly CasaVintageContext _db;
        private readonly IPasswordHasher<Usuario> _hasher;

        public AuthService(CasaVintageContext db, IPasswordHasher<Usuario> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<AuthResultado> ValidarCredencialesAsync(string correo, string password)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario is null)
            {
                return new AuthResultado(ResultadoAutenticacion.NoEncontrado, null);
            }

            var verificacion = _hasher.VerifyHashedPassword(usuario, usuario.Password, password);
            if (verificacion == PasswordVerificationResult.Failed)
            {
                return new AuthResultado(ResultadoAutenticacion.PasswordIncorrecta, null);
            }

            // If the hash was left in an old format, it is recomputed and saved on the fly.
            if (verificacion == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Password = _hasher.HashPassword(usuario, password);
                await _db.SaveChangesAsync();
            }

            // The password is correct: only now is the inactive-account block applied, so the
            // account status is not revealed when the password is wrong.
            if (!usuario.Activo)
            {
                return new AuthResultado(ResultadoAutenticacion.Inactivo, null);
            }

            return new AuthResultado(ResultadoAutenticacion.Exito, usuario);
        }
    }
}
