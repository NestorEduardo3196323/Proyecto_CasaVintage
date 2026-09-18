using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Ventas
{
    // "My sales": the history of the sales registered by the signed-in salesperson.
    [Authorize(Roles = "Vendedor")]
    public class MisVentasModel : PageModel
    {
        private readonly IReporteService _reportes;

        public MisVentasModel(IReporteService reportes)
        {
            _reportes = reportes;
        }

        public IReadOnlyList<VentaHistorialViewModel> Ventas { get; private set; } = System.Array.Empty<VentaHistorialViewModel>();
        public decimal Total { get; private set; }

        public async Task OnGetAsync()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // Only the sales of the signed-in salesperson (filtered by their user id).
            Ventas = await _reportes.HistorialAsync(new ReporteFiltro { IdVendedor = idUsuario });
            Total = Ventas.Sum(v => v.Total);
        }
    }
}
