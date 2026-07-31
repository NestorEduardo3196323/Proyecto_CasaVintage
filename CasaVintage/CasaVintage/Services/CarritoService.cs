using System.Text.Json;
using CasaVintage.Data;
using CasaVintage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CasaVintage.Services
{
    // Implementacion del carrito sobre la sesion. Se guarda solo (idProducto, cantidad); el precio,
    // el stock y la foto se leen frescos de la base al armar el carrito o al cobrar, para no cobrar
    // precios viejos ni vender mas de lo que hay.
    public class CarritoService : ICarritoService
    {
        private const string Clave = "Carrito";

        private readonly IHttpContextAccessor _http;
        private readonly CasaVintageContext _db;

        public CarritoService(IHttpContextAccessor http, CasaVintageContext db)
        {
            _http = http;
            _db = db;
        }

        // Una linea guardada en la sesion.
        private sealed record Linea(int IdProducto, int Cantidad);

        public async Task<ResultadoCarrito> AgregarAsync(int idProducto, int cantidad = 1)
        {
            if (cantidad < 1)
            {
                cantidad = 1;
            }

            var producto = await _db.Productos
                .AsNoTracking()
                .Select(p => new { p.IdProducto, p.Nombre, p.Stock })
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (producto is null)
            {
                return new ResultadoCarrito(false, Contar(), "El producto ya no existe.");
            }
            if (producto.Stock <= 0)
            {
                return new ResultadoCarrito(false, Contar(), "Ese producto esta sin stock.");
            }

            var lineas = Leer();
            var actual = lineas.FirstOrDefault(l => l.IdProducto == idProducto)?.Cantidad ?? 0;
            var nueva = Math.Min(actual + cantidad, producto.Stock);

            lineas.RemoveAll(l => l.IdProducto == idProducto);
            lineas.Add(new Linea(idProducto, nueva));
            Guardar(lineas);

            var mensaje = nueva == actual
                ? $"Ya tienes el maximo disponible de \"{producto.Nombre}\" ({producto.Stock})."
                : $"\"{producto.Nombre}\" agregado al carrito.";
            return new ResultadoCarrito(true, lineas.Sum(l => l.Cantidad), mensaje);
        }

        public void Actualizar(int idProducto, int cantidad)
        {
            var lineas = Leer();
            lineas.RemoveAll(l => l.IdProducto == idProducto);
            if (cantidad > 0)
            {
                lineas.Add(new Linea(idProducto, cantidad));
            }
            Guardar(lineas);
        }

        public void Quitar(int idProducto)
        {
            var lineas = Leer();
            lineas.RemoveAll(l => l.IdProducto == idProducto);
            Guardar(lineas);
        }

        public void Vaciar()
        {
            _http.HttpContext?.Session.Remove(Clave);
        }

        public async Task<CarritoViewModel> ObtenerAsync()
        {
            var lineas = Leer();
            if (lineas.Count == 0)
            {
                return new CarritoViewModel(Array.Empty<CarritoItemViewModel>());
            }

            var ids = lineas.Select(l => l.IdProducto).ToList();
            var productos = await _db.Productos
                .AsNoTracking()
                .Where(p => ids.Contains(p.IdProducto))
                .ToListAsync();

            var items = new List<CarritoItemViewModel>();
            foreach (var linea in lineas)
            {
                var p = productos.FirstOrDefault(x => x.IdProducto == linea.IdProducto);
                if (p is null)
                {
                    continue; // el producto se elimino; se ignora esa linea
                }
                items.Add(new CarritoItemViewModel(
                    p.IdProducto, p.Sku, p.Nombre, p.Foto1, p.Precio, linea.Cantidad, p.Stock));
            }

            return new CarritoViewModel(items);
        }

        public int Contar()
        {
            return Leer().Sum(l => l.Cantidad);
        }

        // ---- Lectura/escritura de la sesion ----
        private List<Linea> Leer()
        {
            var json = _http.HttpContext?.Session.GetString(Clave);
            if (string.IsNullOrEmpty(json))
            {
                return new List<Linea>();
            }
            return JsonSerializer.Deserialize<List<Linea>>(json) ?? new List<Linea>();
        }

        private void Guardar(List<Linea> lineas)
        {
            _http.HttpContext?.Session.SetString(Clave, JsonSerializer.Serialize(lineas));
        }
    }
}
