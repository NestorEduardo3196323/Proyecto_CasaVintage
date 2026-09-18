namespace CasaVintage.ViewModels
{
    // Projection of an employee for the "Company staff" view. Built in the PageModel from the entity
    // (the raw entity is never passed to the view) and never includes the password.
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
