using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Result of sending the invoice by email.
    public sealed record ResultadoEnvio(bool Exito, string? Correo = null, string? Mensaje = null);

    // Generates the receipt (internal invoice) of a sale: data for the view, PDF (QuestPDF) and
    // sending by email (SMTP).
    public interface IFacturaService
    {
        // Receipt data of a sale (company, customer, lines, VAT breakdown). Null if it does not exist.
        Task<FacturaViewModel?> ObtenerFacturaAsync(int idVenta);

        // Generates the receipt PDF. Null if the sale does not exist.
        Task<byte[]?> GenerarPdfAsync(int idVenta);

        // Sends the receipt PDF to the email of the customer registered in the sale.
        Task<ResultadoEnvio> EnviarPorCorreoAsync(int idVenta);

        // Generates the receipt PDF and saves it to the invoices folder on disk.
        Task GuardarEnCarpetaAsync(int idVenta);
    }
}
