using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Confirmation of an already-registered sale: sale number, customer, products, total and method.
    // The receipt (print / PDF / email) is Increment 8.
    [Authorize(Roles = "Vendedor")]
    public class ConfirmacionModel : PageModel
    {
        private readonly IVentaService _ventas;

        public ConfirmacionModel(IVentaService ventas)
        {
            _ventas = ventas;
        }

        public VentaConfirmacionViewModel Venta { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var venta = await _ventas.ObtenerConfirmacionAsync(id);
            if (venta is null)
            {
                return NotFound();
            }

            Venta = venta;
            return Page();
        }
    }
}
