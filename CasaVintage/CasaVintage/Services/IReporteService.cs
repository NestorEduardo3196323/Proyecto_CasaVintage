using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Consultas de reportes para el Contador: resumen del negocio, productos mas/menos vendidos,
    // ventas por vendedor e historial. La rentabilidad se calcula con precio de venta menos costo.
    // Todos los metodos reciben un ReporteFiltro que se aplica dentro de las consultas de EF Core.
    public interface IReporteService
    {
        Task<ReporteResumenViewModel> ObtenerResumenAsync(ReporteFiltro filtro);
        Task<IReadOnlyList<ProductoVendidoViewModel>> MasVendidosAsync(ReporteFiltro filtro, int top = 5);
        Task<IReadOnlyList<ProductoVendidoViewModel>> MenosVendidosAsync(ReporteFiltro filtro, int top = 5);
        Task<IReadOnlyList<VentaPorVendedorViewModel>> VentasPorVendedorAsync(ReporteFiltro filtro);

        // Historial de ventas segun el filtro (por rango de fechas, vendedor, metodo y categoria).
        Task<IReadOnlyList<VentaHistorialViewModel>> HistorialAsync(ReporteFiltro filtro);

        // Opciones para los desplegables del filtro: vendedores (rol Vendedor) y categorias.
        Task<IReadOnlyList<VendedorOpcionViewModel>> VendedoresAsync();
        Task<IReadOnlyList<string>> CategoriasAsync();

        // Exporta el reporte (resumen + productos + vendedores) respetando el filtro activo.
        Task<byte[]> GenerarPdfAsync(ReporteFiltro filtro);
        Task<byte[]> GenerarExcelAsync(ReporteFiltro filtro);
    }
}
