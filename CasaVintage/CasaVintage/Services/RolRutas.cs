namespace CasaVintage.Services
{
    // Decides where each user lands after signing in. There are no dashboards: navigation is only
    // through the side menu (which changes by role). Everyone goes straight to their first available
    // section; if they do not have any built yet, they go to a welcome screen.
    public static class RolRutas
    {
        // Filler screen while the role has no section enabled yet.
        public const string Bienvenida = "/Bienvenida";

        public static string LandingPara(string? rol) => MenuRol.PrimeraDisponible(rol) ?? Bienvenida;
    }
}
