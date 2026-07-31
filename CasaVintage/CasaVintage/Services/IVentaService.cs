using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Motivo por el que una venta no se pudo procesar.
    public enum ErrorVenta
    {
        Ninguno,
        CarritoVacio,
        StockInsuficiente,
        Conflicto,       // otro vendedor cambio el stock a la vez (concurrencia optimista)
        DatosInvalidos,
        Error
    }

    // Resultado de procesar una venta. En exito trae el id de la venta; en falla, el motivo y un
    // detalle (por ejemplo el nombre del producto sin stock suficiente).
    public sealed record ResultadoVenta(bool Exito, int IdVenta = 0, ErrorVenta Error = ErrorVenta.Ninguno, string? Detalle = null);

    // Procesa la venta a partir del carrito, en una sola transaccion, con concurrencia optimista.
    public interface IVentaService
    {
        // Registra la venta: inserta cliente, venta y detalle, descuenta stock y sincroniza
        // disponibilidad, todo en UNA transaccion. Si algo falla, revierte todo. Vacia el carrito
        // al terminar bien. tarjetaUltimos4 son los ultimos 4 digitos cuando el pago es con tarjeta
        // (o null); nunca se recibe ni se guarda el numero completo ni el CVV.
        Task<ResultadoVenta> ProcesarVentaAsync(string clienteNombre, string clienteCorreo, string metodoPago, int idUsuario, string? tarjetaUltimos4 = null);

        // Carga el resumen de una venta ya registrada (para la confirmacion). Null si no existe.
        Task<VentaConfirmacionViewModel?> ObtenerConfirmacionAsync(int idVenta);
    }
}
