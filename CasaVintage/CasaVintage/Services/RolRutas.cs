namespace CasaVintage.Services
{
    // Decide a donde aterriza cada usuario tras iniciar sesion. Ya no hay dashboards: la navegacion
    // es solo por el menu lateral (que cambia segun el rol). Cada quien entra directo a su primera
    // seccion disponible; si aun no tiene ninguna construida, va a una pantalla de bienvenida.
    public static class RolRutas
    {
        // Pantalla de relleno mientras el rol no tiene ninguna seccion habilitada todavia.
        public const string Bienvenida = "/Bienvenida";

        public static string LandingPara(string? rol) => MenuRol.PrimeraDisponible(rol) ?? Bienvenida;
    }
}
