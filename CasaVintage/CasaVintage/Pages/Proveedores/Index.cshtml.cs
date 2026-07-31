using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Proveedores
{
    // Lista de proveedores (Admin/Gerente). Tabla de gestion con acciones. El borrado usa
    // POST-redirect-GET y esta protegido en el service (no se borra un proveedor con productos).
    [Authorize(Roles = "Administrador,Gerente")]
    public class IndexModel : PageModel
    {
        private readonly IProveedorService _proveedores;

        public IndexModel(IProveedorService proveedores)
        {
            _proveedores = proveedores;
        }

        public IReadOnlyList<ProveedorListItemViewModel> Proveedores { get; private set; } = Array.Empty<ProveedorListItemViewModel>();

        // Metricas para las tarjetas de resumen (se calculan de la misma lista, sin consultas extra).
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
                TempData["MensajeProveedor"] = $"Proveedor \"{resultado.Proveedor!.Nombre}\" eliminado.";
            }
            else
            {
                TempData["MensajeProveedorError"] = resultado.Error switch
                {
                    ErrorProveedor.TieneProductos => "No se puede eliminar: el proveedor tiene productos asociados. Reasigna o quita esos productos primero.",
                    ErrorProveedor.NoEncontrado => "El proveedor ya no existe.",
                    _ => "No se pudo eliminar el proveedor. Intenta de nuevo."
                };
            }

            return RedirectToPage();
        }
    }
}
