namespace CasaVintage.Services
{
    // Saves files (invoices, reports) in disk folders configured under "Rutas" in appsettings,
    // relative to the project root. Creates the folder if it does not exist. It is not critical:
    // if something fails, it is logged and continues (it must not bring down the sale or the download).
    public interface IArchivadorLocal
    {
        // Saves the content in the folder named by its key ("Facturas" or "Reportes").
        // Returns the full path where it was saved, or null if it could not.
        Task<string?> GuardarAsync(string claveCarpeta, string nombreArchivo, byte[] contenido);

        // Creates (if it does not exist) the key's folder and returns its absolute path.
        string AsegurarCarpeta(string claveCarpeta);
    }

    public class ArchivadorLocal : IArchivadorLocal
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly IConfiguration _config;
        private readonly ILogger<ArchivadorLocal> _logger;

        public ArchivadorLocal(IWebHostEnvironment entorno, IConfiguration config, ILogger<ArchivadorLocal> logger)
        {
            _entorno = entorno;
            _config = config;
            _logger = logger;
        }

        // Resolves the absolute path of a key's folder (Rutas:<clave> in appsettings, relative to the
        // project root) or a default value.
        private string ResolverCarpeta(string claveCarpeta)
        {
            var configurada = _config[$"Rutas:{claveCarpeta}"] ?? $"..\\{claveCarpeta}";
            return Path.GetFullPath(Path.Combine(_entorno.ContentRootPath, configurada));
        }

        public string AsegurarCarpeta(string claveCarpeta)
        {
            var carpeta = ResolverCarpeta(claveCarpeta);
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        public async Task<string?> GuardarAsync(string claveCarpeta, string nombreArchivo, byte[] contenido)
        {
            try
            {
                var carpeta = ResolverCarpeta(claveCarpeta);
                Directory.CreateDirectory(carpeta);

                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
                await File.WriteAllBytesAsync(rutaCompleta, contenido);

                _logger.LogInformation("File saved at {Ruta}.", rutaCompleta);
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save {Archivo} in the folder {Carpeta}.", nombreArchivo, claveCarpeta);
                return null;
            }
        }
    }
}
