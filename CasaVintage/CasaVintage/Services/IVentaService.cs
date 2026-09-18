using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Reason a sale could not be processed.
    public enum ErrorVenta
    {
        Ninguno,
        CarritoVacio,
        StockInsuficiente,
        Conflicto,       // another salesperson changed the stock at the same time (optimistic concurrency)
        DatosInvalidos,
        Error
    }

    // Result of processing a sale. On success it carries the sale id; on failure, the reason and a
    // detail (for example the name of the product without enough stock).
    public sealed record ResultadoVenta(bool Exito, int IdVenta = 0, ErrorVenta Error = ErrorVenta.Ninguno, string? Detalle = null);

    // Processes the sale from the cart, in a single transaction, with optimistic concurrency.
    public interface IVentaService
    {
        // Registers the sale: inserts customer, sale and detail, decrements stock and syncs
        // availability, all in ONE transaction. If something fails, it rolls back everything. It
        // empties the cart on success. tarjetaUltimos4 are the last 4 digits when the payment is by
        // card (or null); the full number and the CVV are never received nor stored.
        Task<ResultadoVenta> ProcesarVentaAsync(string clienteNombre, string clienteCorreo, string metodoPago, int idUsuario, string? tarjetaUltimos4 = null);

        // Loads the summary of an already-registered sale (for the confirmation). Null if it does not exist.
        Task<VentaConfirmacionViewModel?> ObtenerConfirmacionAsync(int idVenta);
    }
}
