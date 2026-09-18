namespace CasaVintage.Models
{
    // Entity representing an employee with access to the system. Maps to "usuarios".
    // Login is by email; the password is stored hashed (never in plain text).
    public class Usuario
    {
        public int IdUsuario { get; set; }

        // Full name of the employee.
        public string NombreUsuario { get; set; } = string.Empty;

        // Email used to log in (unique).
        public string Correo { get; set; } = string.Empty;

        // Password hash (IPasswordHasher).
        public string Password { get; set; } = string.Empty;

        // Role: Administrador | Gerente | Vendedor | Contador (validated by a CHECK in the DB).
        public string Rol { get; set; } = string.Empty;

        // Active account; if false, access is blocked even when the password is correct.
        public bool Activo { get; set; } = true;

        // Account creation date (audit).
        public DateTime FechaCreado { get; set; }

        // Path on disk of the employee's profile photo (not the binary). Null if there is no photo;
        // in that case the interface shows an avatar with the initials.
        public string? Foto { get; set; }

        // Sales registered by this user (salesperson).
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
