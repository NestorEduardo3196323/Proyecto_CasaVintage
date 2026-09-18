using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Proveedores
{
    // Supplier editing. Admin and Manager. The PageModel only orchestrates.
    [Authorize(Roles = "Administrador,Gerente")]
    public class EditarModel : PageModel
    {
        private readonly IProveedorService _proveedores;

        public EditarModel(IProveedorService proveedores)
        {
            _proveedores = proveedores;
        }

        [BindProperty]
        public ProveedorFormViewModel Entrada { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var proveedor = await _proveedores.ObtenerAsync(id);
            if (proveedor is null)
            {
                return NotFound();
            }

            Entrada = new ProveedorFormViewModel
            {
                IdProveedor = proveedor.IdProveedor,
                Nombre = proveedor.Nombre,
                Contacto = proveedor.Contacto,
                Telefono = proveedor.Telefono
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _proveedores.EditarAsync(
                Entrada.IdProveedor, Entrada.Nombre, Entrada.Contacto, Entrada.Telefono);

            if (!resultado.Exito)
            {
                var mensaje = resultado.Error == ErrorProveedor.NoEncontrado
                    ? "The supplier no longer exists."
                    : "The supplier could not be updated. Try again.";
                ModelState.AddModelError(string.Empty, mensaje);
                return Page();
            }

            TempData["MensajeProveedor"] = $"Supplier \"{resultado.Proveedor!.Nombre}\" updated.";
            return RedirectToPage("Index");
        }
    }
}
