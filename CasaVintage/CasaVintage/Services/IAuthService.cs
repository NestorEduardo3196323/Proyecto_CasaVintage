using CasaVintage.Models;

namespace CasaVintage.Services
{
    // Posibles resultados al validar unas credenciales. NoEncontrado y PasswordIncorrecta se
    // muestran al usuario con el mismo mensaje generico para no revelar que correos existen.
    public enum ResultadoAutenticacion
    {
        Exito,
        NoEncontrado,
        PasswordIncorrecta,
        Inactivo
    }

    // Resultado de validar credenciales: el estado y, si tuvo exito, el usuario autenticado.
    public sealed record AuthResultado(ResultadoAutenticacion Estado, Usuario? Usuario)
    {
        public bool Exito => Estado == ResultadoAutenticacion.Exito;
    }

    // Contrato del servicio de autenticacion. Aisla la logica (buscar usuario, verificar hash,
    // bloquear inactivos) de la PageModel, que solo orquesta.
    public interface IAuthService
    {
        Task<AuthResultado> ValidarCredencialesAsync(string correo, string password);
    }
}
