using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Comprobante (factura interna) de una venta: se puede ver e imprimir aqui, descargar en PDF o
    // enviar por correo. Admin, Vendedor y Contador pueden VER/imprimir/descargar (consulta), pero
    // solo el Vendedor puede ENVIARLO por correo al cliente (accion hacia afuera).
    [Authorize(Roles = "Administrador,Vendedor,Contador,Gerente")]
    public class FacturaModel : PageModel
    {
        private readonly IFacturaService _facturas;
        private readonly IArchivadorLocal _archivador;

        public FacturaModel(IFacturaService facturas, IArchivadorLocal archivador)
        {
            _facturas = facturas;
            _archivador = archivador;
        }

        public FacturaViewModel Factura { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var factura = await _facturas.ObtenerFacturaAsync(id);
            if (factura is null)
            {
                return NotFound();
            }
            Factura = factura;
            return Page();
        }

        // Descarga del comprobante en PDF y guarda ademas una copia en la carpeta de Facturas.
        public async Task<IActionResult> OnGetPdfAsync(int id)
        {
            var pdf = await _facturas.GenerarPdfAsync(id);
            if (pdf is null)
            {
                return NotFound();
            }
            await _archivador.GuardarAsync("Facturas", $"factura-{id:D7}.pdf", pdf);
            return File(pdf, "application/pdf", $"factura-{id:D7}.pdf");
        }

        // Envia el comprobante al correo del cliente. Solo el Vendedor (blindaje del lado servidor).
        public async Task<IActionResult> OnPostEnviarAsync(int id)
        {
            if (!User.IsInRole("Vendedor"))
            {
                TempData["FacturaError"] = "Solo el vendedor puede enviar el comprobante por correo.";
                return RedirectToPage(new { id });
            }

            var resultado = await _facturas.EnviarPorCorreoAsync(id);
            TempData[resultado.Exito ? "FacturaOk" : "FacturaError"] = resultado.Exito
                ? $"Comprobante enviado a {resultado.Correo}."
                : (resultado.Mensaje ?? "No se pudo enviar el comprobante.");
            return RedirectToPage(new { id });
        }
    }
}
