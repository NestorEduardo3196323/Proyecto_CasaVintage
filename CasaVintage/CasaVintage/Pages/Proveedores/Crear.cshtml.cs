using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Proveedores
{
    // Alta de un proveedor. Admin y Gerente. La PageModel solo orquesta.
    [Authorize(Roles = "Administrador,Gerente")]
    public class CrearModel : PageModel
    {
        private readonly IProveedorService _proveedores;

        public CrearModel(IProveedorService proveedores)
        {
            _proveedores = proveedores;
        }

        [BindProperty]
        public ProveedorFormViewModel Entrada { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _proveedores.CrearAsync(Entrada.Nombre, Entrada.Contacto, Entrada.Telefono);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear el proveedor. Intenta de nuevo.");
                return Page();
            }

            TempData["MensajeProveedor"] = $"Proveedor \"{resultado.Proveedor!.Nombre}\" agregado.";
            return RedirectToPage("Index");
        }
    }
}
