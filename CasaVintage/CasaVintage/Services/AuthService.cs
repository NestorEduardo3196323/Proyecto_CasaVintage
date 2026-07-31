using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Servicio de autenticacion. Valida el correo y la contrasena contra la base de datos usando
    // IPasswordHasher (nunca compara texto plano) y aplica la regla de negocio de bloquear cuentas
    // inactivas. No maneja la cookie de sesion; de eso se encarga la PageModel del login.
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

            // Si el hash quedo con un formato viejo, se vuelve a calcular y se guarda al vuelo.
            if (verificacion == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Password = _hasher.HashPassword(usuario, password);
                await _db.SaveChangesAsync();
            }

            // La contrasena es correcta: recien aqui se aplica el bloqueo de cuentas inactivas,
            // para no revelar el estado de la cuenta ante una contrasena equivocada.
            if (!usuario.Activo)
            {
                return new AuthResultado(ResultadoAutenticacion.Inactivo, null);
            }

            return new AuthResultado(ResultadoAutenticacion.Exito, usuario);
        }
    }
}
