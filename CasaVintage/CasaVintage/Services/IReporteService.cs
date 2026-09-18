using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Report queries for the Accountant: business summary, best/least sold products, sales by
    // salesperson and history. Profitability is computed as sale price minus cost.
    // Every method receives a ReporteFiltro that is applied inside the EF Core queries.
    public interface IReporteService
    {
        Task<ReporteResumenViewModel> ObtenerResumenAsync(ReporteFiltro filtro);
        Task<IReadOnlyList<ProductoVendidoViewModel>> MasVendidosAsync(ReporteFiltro filtro, int top = 5);
        Task<IReadOnlyList<ProductoVendidoViewModel>> MenosVendidosAsync(ReporteFiltro filtro, int top = 5);
        Task<IReadOnlyList<VentaPorVendedorViewModel>> VentasPorVendedorAsync(ReporteFiltro filtro);

        // Sales history according to the filter (by date range, salesperson, method and category).
        Task<IReadOnlyList<VentaHistorialViewModel>> HistorialAsync(ReporteFiltro filtro);

        // Options for the filter dropdowns: salespeople (Vendedor role) and categories.
        Task<IReadOnlyList<VendedorOpcionViewModel>> VendedoresAsync();
        Task<IReadOnlyList<string>> CategoriasAsync();

        // Exports the report (summary + products + salespeople) respecting the active filter.
        Task<byte[]> GenerarPdfAsync(ReporteFiltro filtro);
        Task<byte[]> GenerarExcelAsync(ReporteFiltro filtro);
    }
}
