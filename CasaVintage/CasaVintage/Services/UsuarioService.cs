using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Users module implementation. Every account business rule lives here so the PageModels only
    // orchestrate. The email is normalized to lowercase for comparison; the password is always
    // stored hashed with IPasswordHasher (never in plain text).
    public class UsuarioService : IUsuarioService
    {
        private readonly CasaVintageContext _db;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAlmacenArchivos _archivos;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(CasaVintageContext db, IPasswordHasher<Usuario> hasher, IAlmacenArchivos archivos, ILogger<UsuarioService> logger)
        {
            _db = db;
            _hasher = hasher;
            _archivos = archivos;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Usuario>> ListarAsync()
        {
            // Active first, then alphabetical: the staff view shows the current team at the top.
            return await _db.Usuarios
                .AsNoTracking()
                .OrderByDescending(u => u.Activo)
                .ThenBy(u => u.NombreUsuario)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerAsync(int id)
        {
            return await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
        }

        public async Task<ResultadoUsuario> CrearAsync(string nombre, string correo, string rol, string password, string? foto)
        {
            correo = correo.Trim();
            rol = rol.Trim();

            if (!RolInfo.EsValido(rol))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.RolInvalido);
            }

            if (await CorreoEnUsoAsync(correo, exceptoId: null))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.CorreoDuplicado);
            }

            var usuario = new Usuario
            {
                NombreUsuario = nombre.Trim(),
                Correo = correo,
                Rol = rol,
                Activo = true,
                Foto = foto
            };
            usuario.Password = _hasher.HashPassword(usuario, password);

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Account created: {Correo} with role {Rol}.", usuario.Correo, usuario.Rol);
            return ResultadoUsuario.Ok(usuario);
        }

        public async Task<ResultadoUsuario> EditarAsync(int id, string nombre, string correo, string rol, bool cambiarFoto, string? nuevaFoto)
        {
            correo = correo.Trim();
            rol = rol.Trim();

            if (!RolInfo.EsValido(rol))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.RolInvalido);
            }

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoEncontrado);
            }

            if (await CorreoEnUsoAsync(correo, exceptoId: id))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.CorreoDuplicado);
            }

            // Guard: do not leave the system without any active Administrator when demoting the role.
            var quitaAdmin = usuario.Rol == "Administrador" && rol != "Administrador" && usuario.Activo;
            if (quitaAdmin && await EsUltimoAdminActivoAsync(usuario.IdUsuario))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.UltimoAdministrador);
            }

            usuario.NombreUsuario = nombre.Trim();
            usuario.Correo = correo;
            usuario.Rol = rol;

            // Photo replacement: the previous one is deleted from disk and the new path is saved (or null).
            if (cambiarFoto)
            {
                var fotoAnterior = usuario.Foto;
                usuario.Foto = nuevaFoto;
                if (!string.IsNullOrEmpty(fotoAnterior) && fotoAnterior != nuevaFoto)
                {
                    _archivos.Eliminar(fotoAnterior);
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Account edited: id {Id} ({Correo}).", usuario.IdUsuario, usuario.Correo);
            return ResultadoUsuario.Ok(usuario);
        }

        public async Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activar, int idUsuarioActual)
        {
            // The admin cannot enable/disable their own account (prevents accidental self-lockout).
            if (id == idUsuarioActual)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoPuedeCambiarPropioEstado);
            }

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoEncontrado);
            }

            // Guard: do not disable the last active Administrator.
            if (!activar && usuario.Rol == "Administrador" && await EsUltimoAdminActivoAsync(usuario.IdUsuario))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.UltimoAdministrador);
            }

            usuario.Activo = activar;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Account {Estado}: id {Id} ({Correo}).",
                activar ? "enabled" : "disabled", usuario.IdUsuario, usuario.Correo);
            return ResultadoUsuario.Ok(usuario);
        }

        public async Task<ResultadoUsuario> RestablecerPasswordAsync(int id, string nuevaPassword)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoEncontrado);
            }

            usuario.Password = _hasher.HashPassword(usuario, nuevaPassword);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Password reset: id {Id} ({Correo}).", usuario.IdUsuario, usuario.Correo);
            return ResultadoUsuario.Ok(usuario);
        }

        // Checks whether the email is already used by another account (case-insensitive comparison).
        private async Task<bool> CorreoEnUsoAsync(string correo, int? exceptoId)
        {
            return await _db.Usuarios.AnyAsync(u =>
                u.Correo.ToLower() == correo.ToLower() &&
                (exceptoId == null || u.IdUsuario != exceptoId));
        }

        // True if the given account is the only active Administrator left.
        private async Task<bool> EsUltimoAdminActivoAsync(int idUsuario)
        {
            var adminsActivos = await _db.Usuarios
                .CountAsync(u => u.Rol == "Administrador" && u.Activo);
            return adminsActivos <= 1;
        }
    }
}
