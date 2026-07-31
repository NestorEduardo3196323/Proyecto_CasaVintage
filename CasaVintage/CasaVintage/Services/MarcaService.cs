namespace CasaVintage.Services
{
    // Gestiona el logo de la empresa (identidad visual). Se guarda en disco con un nombre fijo
    // (logo.<ext>) dentro de wwwroot/uploads/marca, asi el layout lo encuentra sin necesidad de BD.
    // Solo el Administrador lo cambia (ver Pages/Ajustes/Marca).
    public interface IMarcaService
    {
        // Ruta web del logo actual (con version para refrescar la cache del navegador), o null si no hay.
        string? RutaLogo();

        // Bytes del logo actual para incrustarlo en documentos (PDF, Excel), o null si no hay.
        byte[]? LogoBytes();

        // Guarda el archivo como logo. Devuelve null si todo bien, o un mensaje de error si no es valido.
        Task<string?> GuardarLogoAsync(IFormFile? archivo);

        // Quita el logo actual (la app vuelve al monograma por defecto).
        void QuitarLogo();
    }

    public class MarcaService : IMarcaService
    {
        private static readonly string[] ExtensionesPermitidas = { ".png", ".jpg", ".jpeg", ".webp" };
        private const long TamanoMaximo = 8 * 1024 * 1024; // 8 MB (fotos de celular)

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
                // El parametro ?v= (fecha de modificacion) fuerza a recargar el logo si cambia.
                var version = File.GetLastWriteTimeUtc(archivo).Ticks;
                return $"/uploads/marca/{Path.GetFileName(archivo)}?v={version}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer el logo de la empresa.");
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
                _logger.LogWarning(ex, "No se pudo leer los bytes del logo.");
                return null;
            }
        }

        public async Task<string?> GuardarLogoAsync(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                return "Selecciona una imagen para el logo.";
            }
            if (archivo.Length > TamanoMaximo)
            {
                return "La imagen no debe pasar de 8 MB.";
            }
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return "Formato no valido. Usa PNG, JPG o WEBP.";
            }

            Directory.CreateDirectory(Carpeta);
            // Se borra cualquier logo anterior (sin importar su extension) antes de guardar el nuevo.
            QuitarLogo();

            var destino = Path.Combine(Carpeta, "logo" + extension);
            using (var stream = new FileStream(destino, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }
            return null; // sin error
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
                _logger.LogWarning(ex, "No se pudo quitar el logo de la empresa.");
            }
        }
    }
}
