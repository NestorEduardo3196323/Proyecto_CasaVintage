using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Inventario
{
    // Inventory list (Admin/Manager). Table with thumbnail, SKU, price, stock and availability.
    // Deletion uses POST-redirect-GET and is protected in the service (a sold product is not deleted).
    [Authorize(Roles = "Administrador,Gerente")]
    public class IndexModel : PageModel
    {
        private readonly IProductoService _productos;

        public IndexModel(IProductoService productos)
        {
            _productos = productos;
        }

        public IReadOnlyList<ProductoListItemViewModel> Productos { get; private set; } = Array.Empty<ProductoListItemViewModel>();

        // Metrics for the summary cards (computed from the same list, no extra queries).
        public int Total { get; private set; }
        public int StockBajo { get; private set; }
        public int SinStock { get; private set; }
        public decimal ValorInventario { get; private set; }

        public async Task OnGetAsync()
        {
            Productos = await _productos.ListarAsync();

            Total = Productos.Count;
            // Low stock: at or below the minimum defined for that product (minimum 0 = no alert).
            StockBajo = Productos.Count(p => p.StockMinimo > 0 && p.Stock > 0 && p.Stock <= p.StockMinimo);
            SinStock = Productos.Count(p => p.Stock == 0);
            ValorInventario = Productos.Sum(p => p.Stock * p.Costo);
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _productos.EliminarAsync(id);

            if (resultado.Exito)
            {
                TempData["MensajeInventario"] = $"Product \"{resultado.Producto!.Nombre}\" deleted.";
            }
            else
            {
                TempData["MensajeInventarioError"] = resultado.Error switch
                {
                    ErrorProducto.TieneVentas => "Cannot delete: the product already has registered sales. You can set its stock to 0 so it does not appear as available.",
                    ErrorProducto.NoEncontrado => "The product no longer exists.",
                    _ => "The product could not be deleted. Try again."
                };
            }

            return RedirectToPage();
        }
    }
}
