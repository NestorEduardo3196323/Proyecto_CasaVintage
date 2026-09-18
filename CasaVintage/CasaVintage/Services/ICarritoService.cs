using CasaVintage.ViewModels;

namespace CasaVintage.Services
{
    // Result of an operation on the cart (to respond to the "Add" button AJAX).
    public sealed record ResultadoCarrito(bool Exito, int TotalProductos, string? Mensaje = null);

    // Salesperson shopping cart, stored in the browser session. Each line is a product with its
    // quantity; prices and stock are always read fresh from the database when displaying/charging.
    public interface ICarritoService
    {
        // Adds (or increments) a product to the cart, without going over the available stock.
        Task<ResultadoCarrito> AgregarAsync(int idProducto, int cantidad = 1);

        // Sets a product's quantity (0 or less removes it).
        void Actualizar(int idProducto, int cantidad);

        // Removes a product from the cart.
        void Quitar(int idProducto);

        // Empties the cart completely.
        void Vaciar();

        // Builds the cart with the current data (price, stock, photo) and its totals.
        Task<CarritoViewModel> ObtenerAsync();

        // Total number of units in the cart (for the icon counter). Does not query the database.
        int Contar();
    }
}
