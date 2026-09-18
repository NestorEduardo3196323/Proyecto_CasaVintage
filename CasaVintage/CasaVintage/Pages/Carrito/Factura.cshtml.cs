using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Carrito
{
    // Receipt (internal invoice) of a sale: it can be viewed and printed here, downloaded as PDF or
    // sent by email. Admin, Salesperson and Accountant can VIEW/print/download (read-only), but only
    // the Salesperson can SEND it by email to the customer (outward action).
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

        // Downloads the receipt as PDF and also saves a copy in the Invoices folder.
        public async Task<IActionResult> OnGetPdfAsync(int id)
        {
            var pdf = await _facturas.GenerarPdfAsync(id);
            if (pdf is null)
            {
                return NotFound();
            }
            await _archivador.GuardarAsync("Facturas", $"invoice-{id:D7}.pdf", pdf);
            return File(pdf, "application/pdf", $"invoice-{id:D7}.pdf");
        }

        // Sends the receipt to the customer's email. Only the Salesperson (server-side lockdown).
        public async Task<IActionResult> OnPostEnviarAsync(int id)
        {
            if (!User.IsInRole("Vendedor"))
            {
                TempData["FacturaError"] = "Only the salesperson can send the receipt by email.";
                return RedirectToPage(new { id });
            }

            var resultado = await _facturas.EnviarPorCorreoAsync(id);
            TempData[resultado.Exito ? "FacturaOk" : "FacturaError"] = resultado.Exito
                ? $"Receipt sent to {resultado.Correo}."
                : (resultado.Mensaje ?? "The receipt could not be sent.");
            return RedirectToPage(new { id });
        }
    }
}
