using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Reportes
{
    // Sales history (read-only): Administrator, Accountant and Manager. It respects the filter (date
    // range, salesperson, payment method and category), applied inside the EF Core query.
    [Authorize(Roles = "Administrador,Contador,Gerente")]
    public class HistorialModel : PageModel
    {
        private readonly IReporteService _reportes;

        public HistorialModel(IReporteService reportes)
        {
            _reportes = reportes;
        }

        public IReadOnlyList<VentaHistorialViewModel> Ventas { get; private set; } = System.Array.Empty<VentaHistorialViewModel>();

        // Options and current values of the filter.
        public IReadOnlyList<VendedorOpcionViewModel> Vendedores { get; private set; } = System.Array.Empty<VendedorOpcionViewModel>();
        public IReadOnlyList<string> Categorias { get; private set; } = System.Array.Empty<string>();
        public ReporteFiltro Filtro { get; private set; } = new();
        public string? Rango { get; private set; }
        public int? Vendedor { get; private set; }
        public string? Metodo { get; private set; }
        public string? Categoria { get; private set; }

        public async Task OnGetAsync(string? rango, DateTime? desde, DateTime? hasta, int? vendedor, string? metodo, string? categoria)
        {
            Filtro = ReporteFiltro.Construir(rango, desde, hasta, vendedor, metodo, categoria);
            // The quick-access highlight is computed from the resulting dates (see Index).
            Rango = ReporteFiltro.RangoActivo(Filtro.Desde, Filtro.Hasta);
            Vendedor = vendedor;
            Metodo = string.IsNullOrWhiteSpace(metodo) ? null : metodo;
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria;

            Ventas = await _reportes.HistorialAsync(Filtro);
            Vendedores = await _reportes.VendedoresAsync();
            Categorias = await _reportes.CategoriasAsync();
        }
    }
}
