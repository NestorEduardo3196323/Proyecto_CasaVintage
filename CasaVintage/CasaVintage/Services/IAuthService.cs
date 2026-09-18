using CasaVintage.Models;

namespace CasaVintage.Services
{
    // Possible results when validating credentials. NoEncontrado and PasswordIncorrecta are shown
    // to the user with the same generic message so it does not reveal which emails exist.
    public enum ResultadoAutenticacion
    {
        Exito,
        NoEncontrado,
        PasswordIncorrecta,
        Inactivo
    }

    // Result of validating credentials: the status and, if successful, the authenticated user.
    public sealed record AuthResultado(ResultadoAutenticacion Estado, Usuario? Usuario)
    {
        public bool Exito => Estado == ResultadoAutenticacion.Exito;
    }

    // Authentication service contract. Isolates the logic (find user, verify hash, block inactive)
    // from the PageModel, which only orchestrates.
    public interface IAuthService
    {
        Task<AuthResultado> ValidarCredencialesAsync(string correo, string password);
    }
}
