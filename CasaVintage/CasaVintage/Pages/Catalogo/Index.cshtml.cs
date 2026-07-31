using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Catalogo
{
    // Catalogo de productos: la pantalla de trabajo del Vendedor. El Administrador puede VERLO
    // (consulta), pero NO vender: el boton "Agregar al carrito" no se le muestra y el handler de
    // agregar esta blindado a solo Vendedor. El carrito y el cobro son exclusivos del Vendedor.
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

        // Piezas destacadas para el hero (disponibles). Rotan en el cliente.
        public IReadOnlyList<ProductoCatalogoViewModel> Destacados { get; private set; } = Array.Empty<ProductoCatalogoViewModel>();

        // Valores para los filtros (categoria, epoca, estado), tomados de los productos existentes.
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

        // Handler AJAX del buscador en vivo: devuelve en JSON los productos que coinciden con el
        // texto y los filtros. Lo consume catalogo.js conforme el vendedor escribe o filtra.
        public async Task<IActionResult> OnGetBuscarAsync(string? q, string? categoria, string? epoca, string? estado)
        {
            var resultados = await _productos.BuscarCatalogoAsync(q, categoria, epoca, estado);
            return new JsonResult(resultados);
        }

        // Handler AJAX de "Agregar al carrito": agrega el producto y devuelve el nuevo total y un
        // mensaje. Lo usan las tarjetas del catalogo y la ficha de detalle. Solo el Vendedor puede
        // vender; el Administrador entra al catalogo solo de consulta (blindaje del lado servidor).
        public async Task<IActionResult> OnPostAgregarAsync(int id, int cantidad = 1)
        {
            if (!User.IsInRole("Vendedor"))
            {
                return new JsonResult(new { exito = false, mensaje = "Solo el vendedor puede agregar productos al carrito.", totalProductos = 0 });
            }

            var resultado = await _carrito.AgregarAsync(id, cantidad);
            return new JsonResult(resultado);
        }
    }
}
