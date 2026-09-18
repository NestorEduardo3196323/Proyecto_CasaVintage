using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Sale checkout: captures the customer data and the payment method, and processes the sale.
    // Salesperson only. The PageModel orchestrates: it validates and delegates to IVentaService.
    [Authorize(Roles = "Vendedor")]
    public class PagarModel : PageModel
    {
        private readonly ICarritoService _carrito;
        private readonly IVentaService _ventas;
        private readonly IFacturaService _facturas;

        public PagarModel(ICarritoService carrito, IVentaService ventas, IFacturaService facturas)
        {
            _carrito = carrito;
            _ventas = ventas;
            _facturas = facturas;
        }

        [BindProperty]
        public CobroViewModel Entrada { get; set; } = new();

        public CarritoViewModel Carrito { get; private set; } = new(System.Array.Empty<CarritoItemViewModel>());

        public async Task<IActionResult> OnGetAsync()
        {
            Carrito = await _carrito.ObtenerAsync();
            if (Carrito.Vacio)
            {
                // An empty cart cannot be charged.
                return RedirectToPage("Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Carrito = await _carrito.ObtenerAsync();
            if (Carrito.Vacio)
            {
                return RedirectToPage("Index");
            }

            if (Carrito.HayExcesoStock)
            {
                ModelState.AddModelError(string.Empty, "Some products have more quantity than the available stock. Adjust the cart.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // Only the last 4 digits are passed (simulated card payment); the number and the CVV never leave here.
            var ultimos4 = Entrada.EsTarjeta ? Entrada.Ultimos4() : null;
            var resultado = await _ventas.ProcesarVentaAsync(
                Entrada.ClienteNombre, Entrada.ClienteCorreo, Entrada.MetodoPago, idUsuario, ultimos4);

            if (resultado.Exito)
            {
                // The receipt is archived in the invoices folder (best effort, does not block the sale).
                await _facturas.GuardarEnCarpetaAsync(resultado.IdVenta);
                return RedirectToPage("Confirmacion", new { id = resultado.IdVenta });
            }

            ModelState.AddModelError(string.Empty, MensajeError(resultado));
            return Page();
        }

        private static string MensajeError(ResultadoVenta resultado) => resultado.Error switch
        {
            ErrorVenta.CarritoVacio => "The cart is empty.",
            ErrorVenta.StockInsuficiente => $"There is no longer enough stock of \"{resultado.Detalle}\". Adjust the cart and try again.",
            ErrorVenta.Conflicto => "Another salesperson changed the stock at the same time. Review the cart and try again.",
            ErrorVenta.DatosInvalidos => "Check the payment method.",
            _ => "The sale could not be processed. Try again."
        };
    }
}
