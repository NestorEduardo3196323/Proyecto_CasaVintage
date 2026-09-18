using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Proveedores
{
    // Suppliers list (Admin/Manager). Management table with actions. Deletion uses
    // POST-redirect-GET and is protected in the service (a supplier with products is not deleted).
    [Authorize(Roles = "Administrador,Gerente")]
    public class IndexModel : PageModel
    {
        private readonly IProveedorService _proveedores;

        public IndexModel(IProveedorService proveedores)
        {
            _proveedores = proveedores;
        }

        public IReadOnlyList<ProveedorListItemViewModel> Proveedores { get; private set; } = Array.Empty<ProveedorListItemViewModel>();

        // Metrics for the summary cards (computed from the same list, no extra queries).
        public int Total { get; private set; }
        public int ConProductos { get; private set; }
        public int SinProductos { get; private set; }
        public int ProductosAbastecidos { get; private set; }

        public async Task OnGetAsync()
        {
            Proveedores = await _proveedores.ListarAsync();

            Total = Proveedores.Count;
            ConProductos = Proveedores.Count(p => p.CantidadProductos > 0);
            SinProductos = Total - ConProductos;
            ProductosAbastecidos = Proveedores.Sum(p => p.CantidadProductos);
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _proveedores.EliminarAsync(id);

            if (resultado.Exito)
            {
                TempData["MensajeProveedor"] = $"Supplier \"{resultado.Proveedor!.Nombre}\" deleted.";
            }
            else
            {
                TempData["MensajeProveedorError"] = resultado.Error switch
                {
                    ErrorProveedor.TieneProductos => "Cannot delete: the supplier has associated products. Reassign or remove those products first.",
                    ErrorProveedor.NoEncontrado => "The supplier no longer exists.",
                    _ => "The supplier could not be deleted. Try again."
                };
            }

            return RedirectToPage();
        }
    }
}
