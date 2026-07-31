using CasaVintage.Data;
using CasaVintage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Implementacion del modulo de Usuarios. Toda regla de negocio de las cuentas vive aqui para
    // que las PageModels solo orquesten. El correo se normaliza en minusculas para comparar; la
    // contrasena se guarda siempre hasheada con IPasswordHasher (nunca en texto plano).
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
            // Activos primero y luego alfabetico: la vista del personal muestra al equipo vigente arriba.
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

            _logger.LogInformation("Cuenta creada: {Correo} con rol {Rol}.", usuario.Correo, usuario.Rol);
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

            // Guarda: no dejar el sistema sin ningun Administrador activo al degradar el rol.
            var quitaAdmin = usuario.Rol == "Administrador" && rol != "Administrador" && usuario.Activo;
            if (quitaAdmin && await EsUltimoAdminActivoAsync(usuario.IdUsuario))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.UltimoAdministrador);
            }

            usuario.NombreUsuario = nombre.Trim();
            usuario.Correo = correo;
            usuario.Rol = rol;

            // Reemplazo de foto: se borra del disco la anterior y se guarda la ruta nueva (o null).
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

            _logger.LogInformation("Cuenta editada: id {Id} ({Correo}).", usuario.IdUsuario, usuario.Correo);
            return ResultadoUsuario.Ok(usuario);
        }

        public async Task<ResultadoUsuario> CambiarEstadoAsync(int id, bool activar, int idUsuarioActual)
        {
            // El admin no puede activar/desactivar su propia cuenta (evita auto-bloqueo accidental).
            if (id == idUsuarioActual)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoPuedeCambiarPropioEstado);
            }

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return ResultadoUsuario.Falla(ErrorUsuario.NoEncontrado);
            }

            // Guarda: no desactivar al ultimo Administrador activo.
            if (!activar && usuario.Rol == "Administrador" && await EsUltimoAdminActivoAsync(usuario.IdUsuario))
            {
                return ResultadoUsuario.Falla(ErrorUsuario.UltimoAdministrador);
            }

            usuario.Activo = activar;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Cuenta {Estado}: id {Id} ({Correo}).",
                activar ? "activada" : "desactivada", usuario.IdUsuario, usuario.Correo);
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

            _logger.LogInformation("Contrasena restablecida: id {Id} ({Correo}).", usuario.IdUsuario, usuario.Correo);
            return ResultadoUsuario.Ok(usuario);
        }

        // Comprueba si el correo ya lo usa otra cuenta (comparacion sin distinguir mayusculas).
        private async Task<bool> CorreoEnUsoAsync(string correo, int? exceptoId)
        {
            return await _db.Usuarios.AnyAsync(u =>
                u.Correo.ToLower() == correo.ToLower() &&
                (exceptoId == null || u.IdUsuario != exceptoId));
        }

        // True si la cuenta indicada es el unico Administrador activo que queda.
        private async Task<bool> EsUltimoAdminActivoAsync(int idUsuario)
        {
            var adminsActivos = await _db.Usuarios
                .CountAsync(u => u.Rol == "Administrador" && u.Activo);
            return adminsActivos <= 1;
        }
    }
}
