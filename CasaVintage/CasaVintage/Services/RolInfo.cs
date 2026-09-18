namespace CasaVintage.Services
{
    // Central catalog of the four system roles. Same values enforced by the usuarios.rol CHECK.
    // Centralizing here avoids loose lists: the form <select>, the service validation and the role
    // description in "Company staff" all read from here.
    public static class RolInfo
    {
        // Valid roles, in display order (from widest to narrowest scope).
        public static readonly IReadOnlyList<string> Todos = new[]
        {
            "Administrador", "Gerente", "Vendedor", "Contador"
        };

        // True if the role is one of the allowed ones (avoids values that would break the DB CHECK).
        public static bool EsValido(string? rol) => rol is not null && Todos.Contains(rol);

        // Human-readable description of the role for the staff cards (what each employee does).
        public static string Descripcion(string? rol) => rol switch
        {
            "Administrador" => "Full control of the system. Manages staff accounts, assigns roles and oversees the whole store operation.",
            "Gerente" => "Manages the inventory and suppliers, updates stock and follows up on the business sales.",
            "Vendedor" => "Works the counter: browses the catalog, builds the cart, captures the customer data and processes the sales.",
            "Contador" => "Reviews sales, profitability reports and statistics, and exports the store financial information.",
            _ => "Role without a description."
        };
    }
}
