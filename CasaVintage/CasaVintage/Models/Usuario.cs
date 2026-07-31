namespace CasaVintage.Models
{
    // Entidad que representa a un empleado con acceso al sistema. Mapea a "usuarios".
    // El login es por correo; la contrasena se guarda hasheada (nunca en texto plano).
    public class Usuario
    {
        public int IdUsuario { get; set; }

        // Nombre completo del empleado.
        public string NombreUsuario { get; set; } = string.Empty;

        // Correo con el que inicia sesion (unico).
        public string Correo { get; set; } = string.Empty;

        // Hash de la contrasena (IPasswordHasher).
        public string Password { get; set; } = string.Empty;

        // Rol: Administrador | Gerente | Vendedor | Contador (validado por CHECK en la BD).
        public string Rol { get; set; } = string.Empty;

        // Cuenta activa; si es false el acceso queda bloqueado aunque la contrasena sea correcta.
        public bool Activo { get; set; } = true;

        // Fecha de alta de la cuenta (auditoria).
        public DateTime FechaCreado { get; set; }

        // Ruta en disco de la foto de perfil del empleado (no el binario). Null si no tiene foto;
        // en ese caso la interfaz muestra un avatar con las iniciales.
        public string? Foto { get; set; }

        // Ventas registradas por este usuario (vendedor).
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
