using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Inventario
{
    // Alta de un producto. Admin y Gerente. El SKU se autogenera en el service. Guarda hasta 3
    // fotos en disco; si el alta o una foto falla, borra las imagenes ya guardadas (sin huerfanos).
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

        // Proveedores y sugerencias para el formulario.
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

            // Se guardan las fotos (opcionales) antes de crear. Se lleva una lista para poder
            // borrarlas si el alta falla despues.
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

            TempData["MensajeInventario"] = $"Producto \"{resultado.Producto!.Nombre}\" registrado con SKU {resultado.Producto.Sku}.";
            return RedirectToPage("Index");
        }

        // Guarda una foto opcional; si falla la validacion, marca el error en su campo y devuelve null.
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
            ErrorProducto.ProveedorInvalido => "El proveedor seleccionado no es valido.",
            _ => "No se pudo registrar el producto. Intenta de nuevo."
        };
    }
}
