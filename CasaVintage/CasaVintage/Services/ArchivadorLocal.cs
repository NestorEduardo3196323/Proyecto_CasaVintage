namespace CasaVintage.Services
{
    // Guarda archivos (facturas, reportes) en carpetas del disco configuradas en "Rutas" de
    // appsettings, relativas a la raiz del proyecto. Crea la carpeta si no existe. No es critico:
    // si algo falla, se registra y se sigue (no debe tumbar la venta ni la descarga).
    public interface IArchivadorLocal
    {
        // Guarda el contenido en la carpeta indicada por su clave ("Facturas" o "Reportes").
        // Devuelve la ruta completa donde se guardo, o null si no se pudo.
        Task<string?> GuardarAsync(string claveCarpeta, string nombreArchivo, byte[] contenido);

        // Crea (si no existe) la carpeta de la clave y devuelve su ruta absoluta.
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

        // Resuelve la ruta absoluta de la carpeta de una clave (Rutas:<clave> de appsettings,
        // relativa a la raiz del proyecto) o un valor por defecto.
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

                _logger.LogInformation("Archivo guardado en {Ruta}.", rutaCompleta);
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo guardar {Archivo} en la carpeta {Carpeta}.", nombreArchivo, claveCarpeta);
                return null;
            }
        }
    }
}
