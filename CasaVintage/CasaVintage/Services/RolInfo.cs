namespace CasaVintage.Services
{
    // Catalogo central de los cuatro roles del sistema. Los mismos valores que amarra el CHECK
    // de usuarios.rol. Centralizar aqui evita listas sueltas: el <select> del formulario, la
    // validacion del service y la descripcion del rol en "Personal de la empresa" leen de aqui.
    public static class RolInfo
    {
        // Roles validos, en el orden en que se muestran (mayor a menor alcance).
        public static readonly IReadOnlyList<string> Todos = new[]
        {
            "Administrador", "Gerente", "Vendedor", "Contador"
        };

        // True si el rol es uno de los permitidos (evita valores que romperian el CHECK de la BD).
        public static bool EsValido(string? rol) => rol is not null && Todos.Contains(rol);

        // Descripcion legible del rol para las tarjetas del personal (que hace cada empleado).
        public static string Descripcion(string? rol) => rol switch
        {
            "Administrador" => "Control total del sistema. Gestiona las cuentas del personal, asigna roles y supervisa toda la operacion de la tienda.",
            "Gerente" => "Administra el inventario y los proveedores, actualiza existencias y da seguimiento a las ventas del negocio.",
            "Vendedor" => "Atiende el mostrador: recorre el catalogo, arma el carrito, captura los datos del cliente y procesa las ventas.",
            "Contador" => "Consulta ventas, reportes de rentabilidad y estadisticas, y exporta la informacion financiera de la tienda.",
            _ => "Rol sin descripcion."
        };
    }
}
