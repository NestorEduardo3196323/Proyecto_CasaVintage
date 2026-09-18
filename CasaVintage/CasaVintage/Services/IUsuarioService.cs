using CasaVintage.Models;

namespace CasaVintage.Services
{
    // Reason an operation on an account could not be completed. Lets the PageModel show a precise
    // message (and associate it with the correct field when applicable).
    public enum ErrorUsuario
    {
        Ninguno,
        NoEncontrado,
        CorreoDuplicado,
        RolInvalido,
        NoPuedeCambiarPropioEstado,
        UltimoAdministrador
    }

    // Result of a write operation on an account: success and, if it failed, the reason.
    public sealed record ResultadoUsuario(bool Exito, ErrorUsuario Error = ErrorUsuario.Ninguno, Usuario? Usuario = null)
    {
        public static ResultadoUsuario Ok(Usuario usuario) => new(true, ErrorUsuario.Ninguno, usuario);
        public static ResultadoUsuario Falla(ErrorUsuario error) => new(false, error, null);
    }

    // Contract of the Users / "Company staff" module (Administrator only). It concentrates all the
    // business logic (email uniqueness, password hashing, guards to not leave the system without an
    // administrator or let the admin block themselves). The PageModels only orchestrate: they
    // validate the input, call here and build the ViewModel.
    public interface IUsuarioService
    {
        // All accounts, active first and then by name, for the staff view.
        Task<IReadOnlyList<Usuario>> ListarAsync();

        // A single account by id (null if it does not exist).
        Task<Usuario?> ObtenerAsync(int id);

        // Creates an active account with the hashed password. Fails if the email already exists or
        // the role is not valid. foto is the web path of the already-saved image (or null if none).
        Task<ResultadoUsuario> CrearAsync(string nombre, string correo, string rol, string password, string? foto);

        // Updates name, email and role. Fails if the email clashes with another account, the role is
        // not valid, or the change would leave the system without any active Administrator. If
        // cambiarFoto is true, replaces the photo with nuevaFoto (null = remove it) and deletes the
        // previous one from disk.
        Task<ResultadoUsuario> EditarAsync(int id, string nombre, string correo, string rol, bool cambiarFoto, string? nuevaFoto);

        // Enables or disables the account. It does not let the admin change their own status nor
        // disable the last active Administrator.
        Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activar, int idUsuarioActual);

        // Replaces the password with a new (hashed) one. Fails if the account does not exist.
        Task<ResultadoUsuario> RestablecerPasswordAsync(int id, string nuevaPassword);
    }
}
