namespace CasaVintage.ViewModels
{
    // Proyeccion de un empleado para la vista "Personal de la empresa". Se arma en la PageModel a
    // partir de la entidad (nunca se pasa la entidad cruda a la vista) y jamas incluye la contrasena.
    public sealed record UsuarioCardViewModel(
        int IdUsuario,
        string NombreUsuario,
        string Correo,
        string Rol,
        string Descripcion,
        bool Activo,
        DateTime FechaCreado,
        bool EsUsuarioActual,
        string? Foto);
}
