using System.Security.Claims;

namespace CasaVintage.Services
{
    // Utilidades de presentacion del usuario en sesion (shell). El esquema usuarios no guarda
    // foto de perfil, asi que se muestra un avatar con iniciales calculadas del nombre.
    public static class PerfilUsuario
    {
        // Claim personalizado con la fecha de ingreso (fecha_creado) para la tarjeta de bienvenida.
        public const string ClaimFechaCreado = "FechaCreado";

        // Claim con la ruta de la foto de perfil (si la tiene); el menu y la bienvenida la usan.
        public const string ClaimFoto = "Foto";

        // Ruta de la foto de perfil del usuario en sesion, o null si no tiene (se usan iniciales).
        public static string? Foto(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(ClaimFoto);
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        // Devuelve hasta dos iniciales a partir del nombre completo. "Gerente de Tienda" -> "GT".
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

        // Lee la fecha de ingreso desde los claims; null si no esta presente o no es valida.
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
