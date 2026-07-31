using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Resultado de una operacion sobre el carrito (para responder al AJAX del boton "Agregar").
    public sealed record ResultadoCarrito(bool Exito, int TotalProductos, string? Mensaje = null);

    // Carrito de compra del vendedor, guardado en la sesion del navegador. Cada linea es un producto
    // con su cantidad; los precios y el stock se leen siempre frescos de la base al mostrar/cobrar.
    public interface ICarritoService
    {
        // Agrega (o incrementa) un producto al carrito, sin pasar del stock disponible.
        Task<ResultadoCarrito> AgregarAsync(int idProducto, int cantidad = 1);

        // Fija la cantidad de un producto (0 o menos lo quita).
        void Actualizar(int idProducto, int cantidad);

        // Quita un producto del carrito.
        void Quitar(int idProducto);

        // Vacia el carrito por completo.
        void Vaciar();

        // Arma el carrito con los datos actuales (precio, stock, foto) y sus totales.
        Task<CarritoViewModel> ObtenerAsync();

        // Numero total de unidades en el carrito (para el contador del icono). No consulta la base.
        int Contar();
    }
}
