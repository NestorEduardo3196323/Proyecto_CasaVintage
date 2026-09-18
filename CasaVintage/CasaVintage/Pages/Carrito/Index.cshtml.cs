using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Salesperson cart: reviews the added products, adjusts quantities, removes or empties, and sees
    // the totals. The checkout (customer data + payment method + processing the sale) is the next step.
    [Authorize(Roles = "Vendedor")]
    public class IndexModel : PageModel
    {
        private readonly ICarritoService _carrito;

        public IndexModel(ICarritoService carrito)
        {
            _carrito = carrito;
        }

        public CarritoViewModel Carrito { get; private set; } = new(System.Array.Empty<CarritoItemViewModel>());

        public async Task OnGetAsync()
        {
            Carrito = await _carrito.ObtenerAsync();
        }

        public IActionResult OnPostActualizar(int id, int cantidad)
        {
            _carrito.Actualizar(id, cantidad);
            return RedirectToPage();
        }

        public IActionResult OnPostQuitar(int id)
        {
            _carrito.Quitar(id);
            TempData["MensajeCarrito"] = "Product removed from the cart.";
            return RedirectToPage();
        }

        public IActionResult OnPostVaciar()
        {
            _carrito.Vaciar();
            TempData["MensajeCarrito"] = "Cart emptied.";
            return RedirectToPage();
        }
    }
}
