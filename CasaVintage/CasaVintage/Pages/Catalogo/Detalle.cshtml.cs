using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Catalogo
{
    // Detail card of a catalog product: manual gallery (arrows/thumbnails), all the information
    // and the full description. Admin and Salesperson. The cart is enabled in Increment 7.
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public class DetalleModel : PageModel
    {
        private readonly IProductoService _productos;

        public DetalleModel(IProductoService productos)
        {
            _productos = productos;
        }

        public ProductoDetalleViewModel Producto { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var producto = await _productos.ObtenerDetalleAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            Producto = producto;
            return Page();
        }
    }
}
