namespace CasaVintage.Services
{
    // Implementacion del almacenamiento de imagenes en disco (dentro de wwwroot). Valida el tipo
    // y el tamano antes de escribir, genera un nombre unico para no sobrescribir, y borra por ruta.
    public class AlmacenArchivos : IAlmacenArchivos
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<AlmacenArchivos> _logger;

        // Extensiones y tamano permitidos para las imagenes de perfil.
        private static readonly string[] ExtensionesValidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximoBytes = 8 * 1024 * 1024; // 8 MB (fotos de celular)

        public AlmacenArchivos(IWebHostEnvironment entorno, ILogger<AlmacenArchivos> logger)
        {
            _entorno = entorno;
            _logger = logger;
        }

        public async Task<ResultadoArchivo> GuardarImagenAsync(IFormFile archivo, string subcarpeta)
        {
            if (archivo is null || archivo.Length == 0)
            {
                return new ResultadoArchivo(false, null, ErrorArchivo.Vacio);
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesValidas.Contains(extension) || !archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoArchivo(false, null, ErrorArchivo.FormatoNoValido);
            }

            if (archivo.Length > TamanoMaximoBytes)
            {
                return new ResultadoArchivo(false, null, ErrorArchivo.DemasiadoGrande);
            }

            // wwwroot puede ser null en escenarios raros; se usa una ruta por defecto como respaldo.
            var raiz = _entorno.WebRootPath ?? Path.Combine(_entorno.ContentRootPath, "wwwroot");
            var carpetaDestino = Path.Combine(raiz, subcarpeta.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(carpetaDestino);

            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Ruta web (con barras normales) para persistir y usar en <img src="...">.
            var rutaWeb = "/" + subcarpeta.Trim('/') + "/" + nombreArchivo;
            _logger.LogInformation("Imagen guardada en {Ruta}.", rutaWeb);
            return new ResultadoArchivo(true, rutaWeb, ErrorArchivo.Ninguno);
        }

        public void Eliminar(string? rutaWeb)
        {
            if (string.IsNullOrWhiteSpace(rutaWeb))
            {
                return;
            }

            try
            {
                var raiz = _entorno.WebRootPath ?? Path.Combine(_entorno.ContentRootPath, "wwwroot");
                var rutaRelativa = rutaWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var rutaFisica = Path.Combine(raiz, rutaRelativa);
                if (File.Exists(rutaFisica))
                {
                    File.Delete(rutaFisica);
                    _logger.LogInformation("Imagen eliminada: {Ruta}.", rutaWeb);
                }
            }
            catch (IOException ex)
            {
                // No es critico si no se puede borrar el archivo viejo; se registra y se sigue.
                _logger.LogWarning(ex, "No se pudo eliminar la imagen {Ruta}.", rutaWeb);
            }
        }
    }
}
