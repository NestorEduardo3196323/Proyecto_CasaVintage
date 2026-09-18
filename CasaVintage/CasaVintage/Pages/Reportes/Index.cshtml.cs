using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Reportes
{
    // Reports panel for the Accountant (and the Administrator): summary, best/least-selling products
    // and sales by salesperson. Everything respects the filter (dates, salesperson, payment method
    // and category), applied inside the EF Core queries. The exports use the same filter.
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

        // Options and current values of the filter (for the filter bar and the export links).
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

        // Downloads the report as PDF respecting the filter and saves a copy in the reports folder.
        public async Task<IActionResult> OnGetPdfAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            var filtro = Preparar(rango, desde, hasta, vendedor, metodo, categoria);
            var bytes = await _reportes.GenerarPdfAsync(filtro);
            await _archivador.GuardarAsync("Reportes", $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf", bytes);
            return File(bytes, "application/pdf", "sales-report.pdf");
        }

        // Downloads the report as Excel respecting the filter and saves a copy in the reports folder.
        public async Task<IActionResult> OnGetExcelAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            var filtro = Preparar(rango, desde, hasta, vendedor, metodo, categoria);
            var bytes = await _reportes.GenerarExcelAsync(filtro);
            await _archivador.GuardarAsync("Reportes", $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx", bytes);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sales-report.xlsx");
        }

        // Builds the filter and stores the current values for the view.
        private ReporteFiltro Preparar(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            Filtro = ReporteFiltro.Construir(rango, desde, hasta, vendedor, metodo, categoria);
            // The quick-access highlight is computed from the resulting dates, not from how the form
            // was submitted, so "This month" stays highlighted when another filter changes.
            Rango = ReporteFiltro.RangoActivo(Filtro.Desde, Filtro.Hasta);
            Vendedor = vendedor;
            Metodo = string.IsNullOrWhiteSpace(metodo) ? null : metodo;
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria;
            return Filtro;
        }
    }
}
