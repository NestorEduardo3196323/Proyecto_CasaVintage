using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Inventario
{
    // Lista del inventario (Admin/Gerente). Tabla con miniatura, SKU, precio, stock y disponibilidad.
    // El borrado usa POST-redirect-GET y esta protegido en el service (no se borra un producto vendido).
    [Authorize(Roles = "Administrador,Gerente")]
    public class IndexModel : PageModel
    {
        private readonly IProductoService _productos;

        public IndexModel(IProductoService productos)
        {
            _productos = productos;
        }

        public IReadOnlyList<ProductoListItemViewModel> Productos { get; private set; } = Array.Empty<ProductoListItemViewModel>();

        // Metricas para las tarjetas de resumen (se calculan de la misma lista, sin consultas extra).
        public int Total { get; private set; }
        public int StockBajo { get; private set; }
        public int SinStock { get; private set; }
        public decimal ValorInventario { get; private set; }

        public async Task OnGetAsync()
        {
            Productos = await _productos.ListarAsync();

            Total = Productos.Count;
            // Stock bajo: por debajo (o igual) del minimo definido para ese producto (minimo 0 = sin alerta).
            StockBajo = Productos.Count(p => p.StockMinimo > 0 && p.Stock > 0 && p.Stock <= p.StockMinimo);
            SinStock = Productos.Count(p => p.Stock == 0);
            ValorInventario = Productos.Sum(p => p.Stock * p.Costo);
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _productos.EliminarAsync(id);

            if (resultado.Exito)
            {
                TempData["MensajeInventario"] = $"Producto \"{resultado.Producto!.Nombre}\" eliminado.";
            }
            else
            {
                TempData["MensajeInventarioError"] = resultado.Error switch
                {
                    ErrorProducto.TieneVentas => "No se puede eliminar: el producto ya tiene ventas registradas. Puedes ponerlo en stock 0 para que no aparezca disponible.",
                    ErrorProducto.NoEncontrado => "El producto ya no existe.",
                    _ => "No se pudo eliminar el producto. Intenta de nuevo."
                };
            }

            return RedirectToPage();
        }
    }
}
