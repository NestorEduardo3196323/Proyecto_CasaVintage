using System.Security.Claims;
using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Cobro de la venta: captura los datos del cliente y el metodo de pago, y procesa la venta.
    // Solo el Administrador y el Vendedor. La PageModel orquesta: valida y delega en el IVentaService.
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
                // No se puede cobrar un carrito vacio.
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
                ModelState.AddModelError(string.Empty, "Hay productos con mas cantidad que el stock disponible. Ajusta el carrito.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // Solo se pasan los ultimos 4 digitos (pago con tarjeta simulado); el numero y el CVV no salen de aqui.
            var ultimos4 = Entrada.EsTarjeta ? Entrada.Ultimos4() : null;
            var resultado = await _ventas.ProcesarVentaAsync(
                Entrada.ClienteNombre, Entrada.ClienteCorreo, Entrada.MetodoPago, idUsuario, ultimos4);

            if (resultado.Exito)
            {
                // Se archiva el comprobante en la carpeta de facturas (mejor esfuerzo, no bloquea la venta).
                await _facturas.GuardarEnCarpetaAsync(resultado.IdVenta);
                return RedirectToPage("Confirmacion", new { id = resultado.IdVenta });
            }

            ModelState.AddModelError(string.Empty, MensajeError(resultado));
            return Page();
        }

        private static string MensajeError(ResultadoVenta resultado) => resultado.Error switch
        {
            ErrorVenta.CarritoVacio => "El carrito esta vacio.",
            ErrorVenta.StockInsuficiente => $"Ya no hay stock suficiente de \"{resultado.Detalle}\". Ajusta el carrito e intenta de nuevo.",
            ErrorVenta.Conflicto => "Otro vendedor modifico el stock al mismo tiempo. Revisa el carrito e intenta de nuevo.",
            ErrorVenta.DatosInvalidos => "Revisa el metodo de pago.",
            _ => "No se pudo procesar la venta. Intenta de nuevo."
        };
    }
}
