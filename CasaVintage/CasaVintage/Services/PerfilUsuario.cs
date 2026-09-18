using System.Security.Claims;

namespace CasaVintage.Services
{
    // Presentation helpers for the signed-in user (shell). When the user has no profile photo,
    // an avatar with initials computed from the name is shown.
    public static class PerfilUsuario
    {
        // Custom claim with the join date (fecha_creado) for the welcome card.
        public const string ClaimFechaCreado = "FechaCreado";

        // Claim with the profile photo path (if any); the menu and the welcome card use it.
        public const string ClaimFoto = "Foto";

        // Profile photo path of the signed-in user, or null if none (initials are used instead).
        public static string? Foto(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(ClaimFoto);
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        // Returns up to two initials from the full name. "Store Manager" -> "SM".
        public static string Iniciales(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "?";
            }

            var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1)
            {
                return partes[0].Substring(0, 1).ToUpperInvariant();
            }

            return (partes[0].Substring(0, 1) + partes[^1].Substring(0, 1)).ToUpperInvariant();
        }

        // Reads the join date from the claims; null if not present or not valid.
        public static DateTime? FechaIngreso(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(ClaimFechaCreado);
            if (DateTime.TryParse(valor, null, System.Globalization.DateTimeStyles.RoundtripKind, out var fecha))
            {
                return fecha;
            }

            return null;
        }
    }
}
