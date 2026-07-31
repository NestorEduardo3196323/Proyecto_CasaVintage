using CasaVintage.Services;
using CasaVintage.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CasaVintage.Pages.Inventario
{
    // Edicion de un producto (incluye reabastecer stock). Admin y Gerente. Cada una de las 3 fotos
    // se puede mantener, reemplazar o quitar. El SKU no se edita. Sin archivos huerfanos: las fotos
    // viejas solo se borran tras guardar bien; las recien subidas se borran si el guardado falla.
    [Authorize(Roles = "Administrador,Gerente")]
    public class EditarModel : PageModel
    {
        private readonly IProductoService _productos;
        private readonly IAlmacenArchivos _archivos;

        public EditarModel(IProductoService productos, IAlmacenArchivos archivos)
        {
            _productos = productos;
            _archivos = archivos;
        }

        [BindProperty]
        public ProductoFormViewModel Entrada { get; set; } = new();

        public ProductoFormData Datos { get; private set; } = new(
            Array.Empty<ProveedorOpcion>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var producto = await _productos.ObtenerAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            Datos = await _productos.ObtenerDatosFormularioAsync();
            Entrada = new ProductoFormViewModel
            {
                IdProducto = producto.IdProducto,
                Sku = producto.Sku,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Epoca = producto.Epoca,
                Estado = producto.Estado,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                Costo = producto.Costo,
                Stock = producto.Stock,
                StockMinimo = producto.StockMinimo,
                IdProveedor = producto.IdProveedor,
                Foto1Actual = producto.Foto1,
                Foto2Actual = producto.Foto2,
                Foto3Actual = producto.Foto3
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Datos = await _productos.ObtenerDatosFormularioAsync();

            if (!ModelState.IsValid)
            {
                await RecargarActualesAsync();
                return Page();
            }

            var actual = await _productos.ObtenerAsync(Entrada.IdProducto);
            if (actual is null)
            {
                ModelState.AddModelError(string.Empty, "El producto ya no existe.");
                return Page();
            }

            // Se resuelve cada espacio de foto. "nuevas" se borran si el guardado falla; "aBorrar"
            // (fotos viejas reemplazadas o quitadas) se borran solo tras guardar bien.
            var nuevas = new List<string>();
            var aBorrar = new List<string>();

            var foto1 = await ResolverFotoAsync(Entrada.Foto1, Entrada.QuitarFoto1, actual.Foto1, "Entrada.Foto1", nuevas, aBorrar);
            var foto2 = await ResolverFotoAsync(Entrada.Foto2, Entrada.QuitarFoto2, actual.Foto2, "Entrada.Foto2", nuevas, aBorrar);
            var foto3 = await ResolverFotoAsync(Entrada.Foto3, Entrada.QuitarFoto3, actual.Foto3, "Entrada.Foto3", nuevas, aBorrar);

            if (!ModelState.IsValid)
            {
                nuevas.ForEach(_archivos.Eliminar);
                await RecargarActualesAsync();
                return Page();
            }

            var datos = new ProductoDatos(
                Entrada.Nombre, Entrada.Descripcion, Entrada.Epoca, Entrada.Estado, Entrada.Categoria,
                Entrada.Precio, Entrada.Costo, Entrada.Stock, Entrada.StockMinimo, Entrada.IdProveedor, foto1, foto2, foto3);

            var resultado = await _productos.EditarAsync(Entrada.IdProducto, datos);

            if (!resultado.Exito)
            {
                nuevas.ForEach(_archivos.Eliminar);
                ModelState.AddModelError(string.Empty, MensajeError(resultado.Error));
                await RecargarActualesAsync();
                return Page();
            }

            aBorrar.ForEach(_archivos.Eliminar);
            TempData["MensajeInventario"] = $"Producto \"{resultado.Producto!.Nombre}\" actualizado.";
            return RedirectToPage("Index");
        }

        // Decide la ruta final de un espacio de foto: quitar (null), reemplazar (nueva) o mantener.
        private async Task<string?> ResolverFotoAsync(
            IFormFile? nueva, bool quitar, string? actual, string campo, List<string> nuevas, List<string> aBorrar)
        {
            if (quitar)
            {
                if (!string.IsNullOrEmpty(actual))
                {
                    aBorrar.Add(actual);
                }
                return null;
            }

            if (nueva is not null)
            {
                var resultado = await _archivos.GuardarImagenAsync(nueva, "uploads/productos");
                if (!resultado.Exito)
                {
                    ModelState.AddModelError(campo, MensajeArchivo.Para(resultado.Error));
                    return actual;
                }
                nuevas.Add(resultado.RutaWeb!);
                if (!string.IsNullOrEmpty(actual))
                {
                    aBorrar.Add(actual);
                }
                return resultado.RutaWeb;
            }

            return actual;
        }

        // Vuelve a cargar SKU y fotos actuales desde la base para re-renderizar tras un error.
        private async Task RecargarActualesAsync()
        {
            var producto = await _productos.ObtenerAsync(Entrada.IdProducto);
            if (producto is null)
            {
                return;
            }
            Entrada.Sku = producto.Sku;
            Entrada.Foto1Actual = producto.Foto1;
            Entrada.Foto2Actual = producto.Foto2;
            Entrada.Foto3Actual = producto.Foto3;
        }

        private static string MensajeError(ErrorProducto error) => error switch
        {
            ErrorProducto.ProveedorInvalido => "El proveedor seleccionado no es valido.",
            ErrorProducto.NoEncontrado => "El producto ya no existe.",
            ErrorProducto.Conflicto => "Otro usuario modifico este producto. Vuelve a abrirlo e intenta de nuevo.",
            _ => "No se pudo actualizar el producto. Intenta de nuevo."
        };
    }
}
