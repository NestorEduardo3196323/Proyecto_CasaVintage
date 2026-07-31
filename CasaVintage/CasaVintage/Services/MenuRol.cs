namespace CasaVintage.Services
{
    // Un enlace del menu lateral. Pagina es la ruta Razor (o null si aun no existe).
    // Disponible=false lo pinta como "proximamente" (se habilita en su incremento).
    // Icono es la clave del icono SVG que dibuja el layout (users, box, truck, grid, cart, ...).
    public sealed record MenuItem(string Texto, string? Pagina, bool Disponible, string Icono = "grid");

    // Un grupo de enlaces bajo un titulo de seccion.
    public sealed record MenuSeccion(string Titulo, IReadOnlyList<MenuItem> Items);

    // Define las secciones del menu lateral segun el rol. Es la unica fuente de navegacion: no hay
    // dashboard de inicio; cada rol aterriza directo en su primera seccion disponible (ver
    // PrimeraDisponible). Cada incremento posterior solo cambia Disponible a true y pone la ruta real.
    public static class MenuRol
    {
        public static IReadOnlyList<MenuSeccion> Para(string? rol) => rol switch
        {
            "Administrador" => new[]
            {
                new MenuSeccion("Gestion", new[]
                {
                    new MenuItem("Personal de la empresa", "/Personal/Index", true, "users"),
                    new MenuItem("Inventario", "/Inventario/Index", true, "box"),
                    new MenuItem("Proveedores", "/Proveedores/Index", true, "truck"),
                    new MenuItem("Identidad visual", "/Ajustes/Marca", true, "image")
                }),
                new MenuSeccion("Operacion", new[]
                {
                    new MenuItem("Catalogo", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Ventas", "/Reportes/Historial", true, "cart"),
                    new MenuItem("Reportes", "/Reportes/Index", true, "chart")
                })
            },
            "Gerente" => new[]
            {
                new MenuSeccion("Gestion", new[]
                {
                    new MenuItem("Inventario", "/Inventario/Index", true, "box"),
                    new MenuItem("Proveedores", "/Proveedores/Index", true, "truck")
                }),
                new MenuSeccion("Operacion", new[]
                {
                    new MenuItem("Catalogo", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Ventas", "/Reportes/Historial", true, "cart")
                })
            },
            "Vendedor" => new[]
            {
                new MenuSeccion("Operacion", new[]
                {
                    new MenuItem("Catalogo", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Carrito", "/Carrito/Index", true, "cart"),
                    new MenuItem("Mis ventas", "/Ventas/MisVentas", true, "receipt")
                })
            },
            "Contador" => new[]
            {
                new MenuSeccion("Analisis", new[]
                {
                    new MenuItem("Reportes", "/Reportes/Index", true, "chart"),
                    new MenuItem("Historial de ventas", "/Reportes/Historial", true, "history")
                })
            },
            _ => Array.Empty<MenuSeccion>()
        };

        // Ruta de la primera seccion habilitada del rol (donde aterriza al iniciar sesion), o null
        // si aun no tiene ninguna construida (en ese caso se usa la pantalla de bienvenida).
        public static string? PrimeraDisponible(string? rol)
        {
            return Para(rol)
                .SelectMany(seccion => seccion.Items)
                .FirstOrDefault(item => item.Disponible && item.Pagina is not null)?.Pagina;
        }
    }
}
