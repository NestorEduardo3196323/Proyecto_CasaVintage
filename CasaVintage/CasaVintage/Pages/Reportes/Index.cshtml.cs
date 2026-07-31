using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Reportes
{
    // Panel de reportes del Contador (y del Administrador): resumen, productos mas/menos vendidos y
    // ventas por vendedor. Todo respeta el filtro (fechas, vendedor, metodo de pago y categoria),
    // que se aplica dentro de las consultas de EF Core. Las exportaciones usan el mismo filtro.
    [Authorize(Roles = "Administrador,Contador")]
    public class IndexModel : PageModel
    {
        private readonly IReporteService _reportes;
        private readonly IArchivadorLocal _archivador;

        public IndexModel(IReporteService reportes, IArchivadorLocal archivador)
        {
            _reportes = reportes;
            _archivador = archivador;
        }

        public ReporteResumenViewModel Resumen { get; private set; } = default!;
        public IReadOnlyList<ProductoVendidoViewModel> MasVendidos { get; private set; } = System.Array.Empty<ProductoVendidoViewModel>();
        public IReadOnlyList<ProductoVendidoViewModel> MenosVendidos { get; private set; } = System.Array.Empty<ProductoVendidoViewModel>();
        public IReadOnlyList<VentaPorVendedorViewModel> PorVendedor { get; private set; } = System.Array.Empty<VentaPorVendedorViewModel>();

        // Opciones y valores actuales del filtro (para la barra de filtros y los enlaces de exportar).
        public IReadOnlyList<VendedorOpcionViewModel> Vendedores { get; private set; } = System.Array.Empty<VendedorOpcionViewModel>();
        public IReadOnlyList<string> Categorias { get; private set; } = System.Array.Empty<string>();
        public ReporteFiltro Filtro { get; private set; } = new();
        public string? Rango { get; private set; }
        public int? Vendedor { get; private set; }
        public string? Metodo { get; private set; }
        public string? Categoria { get; private set; }

        public async Task OnGetAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            var filtro = Preparar(rango, desde, hasta, vendedor, metodo, categoria);

            Resumen = await _reportes.ObtenerResumenAsync(filtro);
            MasVendidos = await _reportes.MasVendidosAsync(filtro, 5);
            MenosVendidos = await _reportes.MenosVendidosAsync(filtro, 5);
            PorVendedor = await _reportes.VentasPorVendedorAsync(filtro);
            Vendedores = await _reportes.VendedoresAsync();
            Categorias = await _reportes.CategoriasAsync();
        }

        // Descarga el reporte en PDF respetando el filtro y guarda una copia en la carpeta de reportes.
        public async Task<IActionResult> OnGetPdfAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            var filtro = Preparar(rango, desde, hasta, vendedor, metodo, categoria);
            var bytes = await _reportes.GenerarPdfAsync(filtro);
            await _archivador.GuardarAsync("Reportes", $"reporte-ventas-{DateTime.Now:yyyyMMdd-HHmmss}.pdf", bytes);
            return File(bytes, "application/pdf", "reporte-ventas.pdf");
        }

        // Descarga el reporte en Excel respetando el filtro y guarda una copia en la carpeta de reportes.
        public async Task<IActionResult> OnGetExcelAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            var filtro = Preparar(rango, desde, hasta, vendedor, metodo, categoria);
            var bytes = await _reportes.GenerarExcelAsync(filtro);
            await _archivador.GuardarAsync("Reportes", $"reporte-ventas-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx", bytes);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reporte-ventas.xlsx");
        }

        // Construye el filtro y guarda los valores actuales para la vista.
        private ReporteFiltro Preparar(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            Filtro = ReporteFiltro.Construir(rango, desde, hasta, vendedor, metodo, categoria);
            // El resaltado del acceso rapido se calcula segun las fechas resultantes, no segun como
            // se envio el formulario, para que "Este mes" siga marcado al cambiar otro filtro.
            Rango = ReporteFiltro.RangoActivo(Filtro.Desde, Filtro.Hasta);
            Vendedor = vendedor;
            Metodo = string.IsNullOrWhiteSpace(metodo) ? null : metodo;
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria;
            return Filtro;
        }
    }
}
