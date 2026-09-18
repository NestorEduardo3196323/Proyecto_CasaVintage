namespace CasaVintage.Services
{
    // Reason an image could not be saved (to show a clear message to the user).
    public enum ErrorArchivo
    {
        Ninguno,
        Vacio,
        FormatoNoValido,
        DemasiadoGrande
    }

    // Result of saving an image: the relative web path (e.g. /uploads/usuarios/xxx.jpg) on success,
    // or the reason for the failure.
    public sealed record ResultadoArchivo(bool Exito, string? RutaWeb = null, ErrorArchivo Error = ErrorArchivo.Ninguno);

    // Translates the reason of a rejected image into a message for the user (shared by the modules
    // that upload images: personnel, inventory).
    public static class MensajeArchivo
    {
        public static string Para(ErrorArchivo error) => error switch
        {
            ErrorArchivo.FormatoNoValido => "The image must be JPG, PNG or WEBP.",
            ErrorArchivo.DemasiadoGrande => "The image cannot exceed 8 MB.",
            _ => "The image could not be saved. Try another one."
        };
    }

    // Saves and deletes images on disk under wwwroot. The file path is ALWAYS stored, never the
    // binary in the database (same as product photos). Reusable by other modules.
    public interface IAlmacenArchivos
    {
        // Saves the validated image (type and size) inside wwwroot/<subfolder>. Returns the relative
        // web path to persist in the database and use in <img src="...">.
        Task<ResultadoArchivo> GuardarImagenAsync(IFormFile archivo, string subcarpeta);

        // Deletes from disk the file indicated by its relative web path. Does not fail if it does not exist.
        void Eliminar(string? rutaWeb);
    }
}
