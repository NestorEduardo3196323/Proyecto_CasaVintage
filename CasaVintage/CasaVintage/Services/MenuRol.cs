namespace CasaVintage.Services
{
    // A side-menu link. Pagina is the Razor route (or null if it does not exist yet).
    // Disponible=false renders it as "coming soon" (enabled in its increment).
    // Icono is the key of the SVG icon drawn by the layout (users, box, truck, grid, cart, ...).
    public sealed record MenuItem(string Texto, string? Pagina, bool Disponible, string Icono = "grid");

    // A group of links under a section title.
    public sealed record MenuSeccion(string Titulo, IReadOnlyList<MenuItem> Items);

    // Defines the side-menu sections by role. It is the single source of navigation: there is no
    // home dashboard; each role lands straight in its first available section (see PrimeraDisponible).
    public static class MenuRol
    {
        public static IReadOnlyList<MenuSeccion> Para(string? rol) => rol switch
        {
            "Administrador" => new[]
            {
                new MenuSeccion("Management", new[]
                {
                    new MenuItem("Company staff", "/Personal/Index", true, "users"),
                    new MenuItem("Inventory", "/Inventario/Index", true, "box"),
                    new MenuItem("Suppliers", "/Proveedores/Index", true, "truck"),
                    new MenuItem("Visual identity", "/Ajustes/Marca", true, "image")
                }),
                new MenuSeccion("Operation", new[]
                {
                    new MenuItem("Catalog", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Sales", "/Reportes/Historial", true, "cart"),
                    new MenuItem("Reports", "/Reportes/Index", true, "chart")
                })
            },
            "Gerente" => new[]
            {
                new MenuSeccion("Management", new[]
                {
                    new MenuItem("Inventory", "/Inventario/Index", true, "box"),
                    new MenuItem("Suppliers", "/Proveedores/Index", true, "truck")
                }),
                new MenuSeccion("Operation", new[]
                {
                    new MenuItem("Catalog", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Sales", "/Reportes/Historial", true, "cart")
                })
            },
            "Vendedor" => new[]
            {
                new MenuSeccion("Operation", new[]
                {
                    new MenuItem("Catalog", "/Catalogo/Index", true, "grid"),
                    new MenuItem("Cart", "/Carrito/Index", true, "cart"),
                    new MenuItem("My sales", "/Ventas/MisVentas", true, "receipt")
                })
            },
            "Contador" => new[]
            {
                new MenuSeccion("Analysis", new[]
                {
                    new MenuItem("Reports", "/Reportes/Index", true, "chart"),
                    new MenuItem("Sales history", "/Reportes/Historial", true, "history")
                })
            },
            _ => Array.Empty<MenuSeccion>()
        };

        // Route of the first enabled section for the role (where it lands on sign in), or null if it
        // has none built yet (in that case the welcome screen is used).
        public static string? PrimeraDisponible(string? rol)
        {
            return Para(rol)
                .SelectMany(seccion => seccion.Items)
                .FirstOrDefault(item => item.Disponible && item.Pagina is not null)?.Pagina;
        }
    }
}
