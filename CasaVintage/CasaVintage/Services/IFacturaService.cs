using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Resultado de enviar la factura por correo.
    public sealed record ResultadoEnvio(bool Exito, string? Correo = null, string? Mensaje = null);

    // Genera el comprobante (factura interna) de una venta: datos para la vista, PDF (QuestPDF) y
    // envio por correo (SMTP).
    public interface IFacturaService
    {
        // Datos del comprobante de una venta (empresa, cliente, lineas, desglose de IVA). Null si no existe.
        Task<FacturaViewModel?> ObtenerFacturaAsync(int idVenta);

        // Genera el PDF del comprobante. Null si la venta no existe.
        Task<byte[]?> GenerarPdfAsync(int idVenta);

        // Envia el comprobante en PDF al correo del cliente registrado en la venta.
        Task<ResultadoEnvio> EnviarPorCorreoAsync(int idVenta);

        // Genera el PDF del comprobante y lo guarda en la carpeta de facturas del disco.
        Task GuardarEnCarpetaAsync(int idVenta);
    }
}
