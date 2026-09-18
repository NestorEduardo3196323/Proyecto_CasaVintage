namespace CasaVintage.Services
{
    // Disk image storage implementation (inside wwwroot). Validates the type and size before writing,
    // generates a unique name to avoid overwriting, and deletes by path.
    public class AlmacenArchivos : IAlmacenArchivos
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<AlmacenArchivos> _logger;

        // Allowed extensions and size for the profile images.
        private static readonly string[] ExtensionesValidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximoBytes = 8 * 1024 * 1024; // 8 MB (phone photos)

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

            // wwwroot can be null in rare scenarios; a default path is used as a fallback.
            var raiz = _entorno.WebRootPath ?? Path.Combine(_entorno.ContentRootPath, "wwwroot");
            var carpetaDestino = Path.Combine(raiz, subcarpeta.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(carpetaDestino);

            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Web path (with normal slashes) to persist and use in <img src="...">.
            var rutaWeb = "/" + subcarpeta.Trim('/') + "/" + nombreArchivo;
            _logger.LogInformation("Image saved at {Ruta}.", rutaWeb);
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
                    _logger.LogInformation("Image deleted: {Ruta}.", rutaWeb);
                }
            }
            catch (IOException ex)
            {
                // It is not critical if the old file cannot be deleted; it is logged and continues.
                _logger.LogWarning(ex, "Could not delete the image {Ruta}.", rutaWeb);
            }
        }
    }
}
