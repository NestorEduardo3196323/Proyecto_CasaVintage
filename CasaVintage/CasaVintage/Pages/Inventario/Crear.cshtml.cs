using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Inventario
{
    // Product creation. Admin and Manager. The SKU is auto-generated in the service. It saves up to
    // 3 photos on disk; if the creation or a photo fails, it deletes the already-saved images (no orphans).
    [Authorize(Roles = "Administrador,Gerente")]
    public class CrearModel : PageModel
    {
        private readonly IProductoService _productos;
        private readonly IAlmacenArchivos _archivos;

        public CrearModel(IProductoService productos, IAlmacenArchivos archivos)
        {
            _productos = productos;
            _archivos = archivos;
        }

        [BindProperty]
        public ProductoFormViewModel Entrada { get; set; } = new();

        // Suppliers and suggestions for the form.
        public ProductoFormData Datos { get; private set; } = new(
            Array.Empty<ProveedorOpcion>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        public async Task OnGetAsync()
        {
            Datos = await _productos.ObtenerDatosFormularioAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Datos = await _productos.ObtenerDatosFormularioAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The photos (optional) are saved before creating. A list is kept so they can be
            // deleted if the creation fails afterwards.
            var guardadas = new List<string>();
            var foto1 = await GuardarFotoAsync(Entrada.Foto1, "Entrada.Foto1", guardadas);
            var foto2 = await GuardarFotoAsync(Entrada.Foto2, "Entrada.Foto2", guardadas);
            var foto3 = await GuardarFotoAsync(Entrada.Foto3, "Entrada.Foto3", guardadas);

            if (!ModelState.IsValid)
            {
                guardadas.ForEach(_archivos.Eliminar);
                return Page();
            }

            var datos = ConstruirDatos(foto1, foto2, foto3);
            var resultado = await _productos.CrearAsync(datos);

            if (!resultado.Exito)
            {
                guardadas.ForEach(_archivos.Eliminar);
                ModelState.AddModelError(string.Empty, MensajeError(resultado.Error));
                return Page();
            }

            TempData["MensajeInventario"] = $"Product \"{resultado.Producto!.Nombre}\" registered with SKU {resultado.Producto.Sku}.";
            return RedirectToPage("Index");
        }

        // Saves an optional photo; if validation fails, marks the error in its field and returns null.
        private async Task<string?> GuardarFotoAsync(IFormFile? archivo, string campo, List<string> guardadas)
        {
            if (archivo is null)
            {
                return null;
            }

            var resultado = await _archivos.GuardarImagenAsync(archivo, "uploads/productos");
            if (!resultado.Exito)
            {
                ModelState.AddModelError(campo, MensajeArchivo.Para(resultado.Error));
                return null;
            }

            guardadas.Add(resultado.RutaWeb!);
            return resultado.RutaWeb;
        }

        private ProductoDatos ConstruirDatos(string? foto1, string? foto2, string? foto3) => new(
            Entrada.Nombre, Entrada.Descripcion, Entrada.Epoca, Entrada.Estado, Entrada.Categoria,
            Entrada.Precio, Entrada.Costo, Entrada.Stock, Entrada.StockMinimo, Entrada.IdProveedor, foto1, foto2, foto3);

        private static string MensajeError(ErrorProducto error) => error switch
        {
            ErrorProducto.ProveedorInvalido => "The selected supplier is not valid.",
            _ => "The product could not be registered. Try again."
        };
    }
}
