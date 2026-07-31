namespace CasaVintage.Services
{
    // Motivo por el que una imagen no se pudo guardar (para mostrar un mensaje claro al usuario).
    public enum ErrorArchivo
    {
        Ninguno,
        Vacio,
        FormatoNoValido,
        DemasiadoGrande
    }

    // Resultado de guardar una imagen: la ruta web relativa (p. ej. /uploads/usuarios/xxx.jpg)
    // cuando tuvo exito, o el motivo de la falla.
    public sealed record ResultadoArchivo(bool Exito, string? RutaWeb = null, ErrorArchivo Error = ErrorArchivo.Ninguno);

    // Traduce el motivo de una imagen rechazada a un mensaje para el usuario (compartido por los
    // modulos que suben imagenes: personal, inventario).
    public static class MensajeArchivo
    {
        public static string Para(ErrorArchivo error) => error switch
        {
            ErrorArchivo.FormatoNoValido => "La imagen debe ser JPG, PNG o WEBP.",
            ErrorArchivo.DemasiadoGrande => "La imagen no puede superar los 8 MB.",
            _ => "No se pudo guardar la imagen. Intenta con otra."
        };
    }

    // Guarda y elimina imagenes en disco bajo wwwroot. Se guarda SIEMPRE la ruta del archivo,
    // nunca el binario en la base (igual que las fotos de productos). Reutilizable por otros modulos.
    public interface IAlmacenArchivos
    {
        // Guarda la imagen validada (tipo y tamano) dentro de wwwroot/<subcarpeta>. Devuelve la
        // ruta web relativa para persistir en la base y usar en <img src="...">.
        Task<ResultadoArchivo> GuardarImagenAsync(IFormFile archivo, string subcarpeta);

        // Elimina del disco el archivo indicado por su ruta web relativa. No falla si no existe.
        void Eliminar(string? rutaWeb);
    }
}
