using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Catalogo
{
    // Product catalog: the Salesperson's work screen. The Administrator can VIEW it (read-only),
    // but cannot sell: the "Add to cart" button is not shown to them and the add handler is locked
    // to Salesperson only. The cart and the checkout are exclusive to the Salesperson.
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public class IndexModel : PageModel
    {
        private readonly IProductoService _productos;
        private readonly ICarritoService _carrito;

        public IndexModel(IProductoService productos, ICarritoService carrito)
        {
            _productos = productos;
            _carrito = carrito;
        }

        public IReadOnlyList<ProductoCatalogoViewModel> Productos { get; private set; } = Array.Empty<ProductoCatalogoViewModel>();

        // Featured pieces for the hero (available). They rotate on the client.
        public IReadOnlyList<ProductoCatalogoViewModel> Destacados { get; private set; } = Array.Empty<ProductoCatalogoViewModel>();

        // Values for the filters (category, era, condition), taken from the existing products.
        public IReadOnlyList<string> Categorias { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> Epocas { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> Estados { get; private set; } = Array.Empty<string>();

        public async Task OnGetAsync()
        {
            Productos = await _productos.ListarCatalogoAsync();

            Destacados = Productos.Where(p => p.Disponibilidad).Take(6).ToList();
            Categorias = Productos.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToList();
            Epocas = Productos.Select(p => p.Epoca).Distinct().OrderBy(e => e).ToList();
            Estados = Productos.Select(p => p.Estado).Distinct().OrderBy(e => e).ToList();
        }

        // Live search AJAX handler: returns as JSON the products matching the text and the filters.
        // catalogo.js consumes it as the salesperson types or filters.
        public async Task<IActionResult> OnGetBuscarAsync(string? q, string? categoria, string? epoca, string? estado)
        {
            var resultados = await _productos.BuscarCatalogoAsync(q, categoria, epoca, estado);
            return new JsonResult(resultados);
        }

        // "Add to cart" AJAX handler: adds the product and returns the new total and a message.
        // The catalog cards and the detail card use it. Only the Salesperson can sell; the
        // Administrator enters the catalog read-only (server-side lockdown).
        public async Task<IActionResult> OnPostAgregarAsync(int id, int cantidad = 1)
        {
            if (!User.IsInRole("Vendedor"))
            {
                return new JsonResult(new { exito = false, mensaje = "Only the salesperson can add products to the cart.", totalProductos = 0 });
            }

            var resultado = await _carrito.AgregarAsync(id, cantidad);
            return new JsonResult(resultado);
        }
    }
}
