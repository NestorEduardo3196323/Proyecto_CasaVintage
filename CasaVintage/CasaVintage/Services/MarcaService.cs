namespace CasaVintage.Services
{
    // Manages the company logo (visual identity). It is saved to disk with a fixed name
    // (logo.<ext>) inside wwwroot/uploads/marca, so the layout finds it without needing the DB.
    // Only the Administrator changes it (see Pages/Ajustes/Marca).
    public interface IMarcaService
    {
        // Web path of the current logo (with a version to refresh the browser cache), or null if none.
        string? RutaLogo();

        // Bytes of the current logo to embed it in documents (PDF, Excel), or null if none.
        byte[]? LogoBytes();

        // Saves the file as the logo. Returns null if everything is fine, or an error message if invalid.
        Task<string?> GuardarLogoAsync(IFormFile? archivo);

        // Removes the current logo (the app goes back to the default monogram).
        void QuitarLogo();
    }

    public class MarcaService : IMarcaService
    {
        private static readonly string[] ExtensionesPermitidas = { ".png", ".jpg", ".jpeg", ".webp" };
        private const long TamanoMaximo = 8 * 1024 * 1024; // 8 MB (phone photos)

        private readonly IWebHostEnvironment _entorno;
        private readonly ILogger<MarcaService> _logger;

        public MarcaService(IWebHostEnvironment entorno, ILogger<MarcaService> logger)
        {
            _entorno = entorno;
            _logger = logger;
        }

        private string Carpeta => Path.Combine(_entorno.WebRootPath, "uploads", "marca");

        public string? RutaLogo()
        {
            try
            {
                if (!Directory.Exists(Carpeta))
                {
                    return null;
                }
                var archivo = Directory.GetFiles(Carpeta, "logo.*").FirstOrDefault();
                if (archivo is null)
                {
                    return null;
                }
                // The ?v= parameter (modification date) forces the logo to reload if it changes.
                var version = File.GetLastWriteTimeUtc(archivo).Ticks;
                return $"/uploads/marca/{Path.GetFileName(archivo)}?v={version}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read the company logo.");
                return null;
            }
        }

        public byte[]? LogoBytes()
        {
            try
            {
                if (!Directory.Exists(Carpeta))
                {
                    return null;
                }
                var archivo = Directory.GetFiles(Carpeta, "logo.*").FirstOrDefault();
                return archivo is null ? null : File.ReadAllBytes(archivo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read the logo bytes.");
                return null;
            }
        }

        public async Task<string?> GuardarLogoAsync(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                return "Select an image for the logo.";
            }
            if (archivo.Length > TamanoMaximo)
            {
                return "The image must not exceed 8 MB.";
            }
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return "Invalid format. Use PNG, JPG or WEBP.";
            }

            Directory.CreateDirectory(Carpeta);
            // Any previous logo (regardless of its extension) is deleted before saving the new one.
            QuitarLogo();

            var destino = Path.Combine(Carpeta, "logo" + extension);
            using (var stream = new FileStream(destino, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }
            return null; // no error
        }

        public void QuitarLogo()
        {
            try
            {
                if (!Directory.Exists(Carpeta))
                {
                    return;
                }
                foreach (var previo in Directory.GetFiles(Carpeta, "logo.*"))
                {
                    File.Delete(previo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove the company logo.");
            }
        }
    }
}
