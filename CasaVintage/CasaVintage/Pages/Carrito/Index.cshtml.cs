using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Carrito del vendedor: revisa los productos agregados, ajusta cantidades, quita o vacia, y ve
    // los totales. El cobro (datos del cliente + metodo de pago + procesar la venta) es el paso siguiente.
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
            TempData["MensajeCarrito"] = "Producto quitado del carrito.";
            return RedirectToPage();
        }

        public IActionResult OnPostVaciar()
        {
            _carrito.Vaciar();
            TempData["MensajeCarrito"] = "Carrito vaciado.";
            return RedirectToPage();
        }
    }
}
