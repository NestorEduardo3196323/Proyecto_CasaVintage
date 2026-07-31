using CasaVintage.Models;

namespace CasaVintage.Services
{
    // Motivo por el que una operacion sobre una cuenta no se pudo completar. Permite que la
    // PageModel muestre un mensaje preciso (y lo asocie al campo correcto cuando aplica).
    public enum ErrorUsuario
    {
        Ninguno,
        NoEncontrado,
        CorreoDuplicado,
        RolInvalido,
        NoPuedeCambiarPropioEstado,
        UltimoAdministrador
    }

    // Resultado de una operacion de escritura sobre una cuenta: exito y, si fallo, el motivo.
    public sealed record ResultadoUsuario(bool Exito, ErrorUsuario Error = ErrorUsuario.Ninguno, Usuario? Usuario = null)
    {
        public static ResultadoUsuario Ok(Usuario usuario) => new(true, ErrorUsuario.Ninguno, usuario);
        public static ResultadoUsuario Falla(ErrorUsuario error) => new(false, error, null);
    }

    // Contrato del modulo de Usuarios / "Personal de la empresa" (solo Administrador). Concentra
    // toda la logica de negocio (unicidad de correo, hash de contrasenas, guardas para no dejar el
    // sistema sin administrador ni permitir que el admin se bloquee a si mismo). Las PageModels
    // solo orquestan: validan la entrada, llaman aqui y arman el ViewModel.
    public interface IUsuarioService
    {
        // Todas las cuentas, activas primero y luego por nombre, para la vista del personal.
        Task<IReadOnlyList<Usuario>> ListarAsync();

        // Una cuenta por id (null si no existe).
        Task<Usuario?> ObtenerAsync(int id);

        // Da de alta una cuenta activa con la contrasena hasheada. Falla si el correo ya existe
        // o el rol no es valido. foto es la ruta web de la imagen ya guardada (o null si no tiene).
        Task<ResultadoUsuario> CrearAsync(string nombre, string correo, string rol, string password, string? foto);

        // Actualiza nombre, correo y rol. Falla si el correo choca con otra cuenta, el rol no es
        // valido, o el cambio dejaria al sistema sin ningun Administrador activo. Si cambiarFoto es
        // true, reemplaza la foto por nuevaFoto (null = quitarla) y borra del disco la anterior.
        Task<ResultadoUsuario> EditarAsync(int id, string nombre, string correo, string rol, bool cambiarFoto, string? nuevaFoto);

        // Activa o desactiva la cuenta. No permite que el admin cambie su propio estado ni que se
        // desactive al ultimo Administrador activo.
        Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activar, int idUsuarioActual);

        // Reemplaza la contrasena por una nueva (hasheada). Falla si la cuenta no existe.
        Task<ResultadoUsuario> RestablecerPasswordAsync(int id, string nuevaPassword);
    }
}
