namespace CasaVintage.ViewModels
{
    // Numeros clave del negocio para las tarjetas del reporte.
    public sealed record ReporteResumenViewModel(
        int TotalVentas,
        int UnidadesVendidas,
        decimal Ingresos,
        decimal Ganancia,
        decimal TicketPromedio);

    // Un producto con lo que se ha vendido de el (mas/menos vendidos).
    public sealed record ProductoVendidoViewModel(
        string Sku,
        string Nombre,
        int CantidadVendida,
        decimal Ingresos);

    // Ventas agrupadas por vendedor.
    public sealed record VentaPorVendedorViewModel(
        string Vendedor,
        int NumeroVentas,
        decimal Total);

    // Una fila del historial de ventas.
    public sealed record VentaHistorialViewModel(
        int IdVenta,
        DateTime Fecha,
        string Cliente,
        string Vendedor,
        string MetodoPago,
        int Articulos,
        decimal Total);

    // Un vendedor para el desplegable de filtro (id de usuario + nombre).
    public sealed record VendedorOpcionViewModel(int Id, string Nombre);

    // Filtro de los reportes del Contador. Se aplica DENTRO de las consultas de EF Core (no en
    // memoria): rango de fechas sobre ventas.fecha, vendedor por ventas.id_usuario, metodo de pago
    // y categoria de producto. Cualquier combinacion es opcional.
    public sealed class ReporteFiltro
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdVendedor { get; set; }
        public string? MetodoPago { get; set; }
        public string? Categoria { get; set; }

        public bool HayFiltro =>
            Desde.HasValue || Hasta.HasValue || IdVendedor.HasValue
            || !string.IsNullOrEmpty(MetodoPago) || !string.IsNullOrEmpty(Categoria);

        // Construye el filtro a partir de los parametros de la peticion. "rango" son los accesos
        // rapidos (hoy, semana, mes, anio); si no es uno de ellos se usan las fechas desde/hasta.
        public static ReporteFiltro Construir(string? rango, DateTime? desde, DateTime? hasta,
            int? vendedor, string? metodo, string? categoria)
        {
            var hoy = DateTime.Today;
            switch (rango)
            {
                case "hoy": desde = hoy; hasta = hoy; break;
                case "semana": desde = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7)); hasta = hoy; break; // desde el lunes
                case "mes": desde = new DateTime(hoy.Year, hoy.Month, 1); hasta = hoy; break;
                case "anio": desde = new DateTime(hoy.Year, 1, 1); hasta = hoy; break;
            }
            return new ReporteFiltro
            {
                Desde = desde,
                Hasta = hasta,
                IdVendedor = vendedor,
                MetodoPago = string.IsNullOrWhiteSpace(metodo) ? null : metodo,
                Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria
            };
        }

        // Devuelve el acceso rapido que coincide con el rango de fechas actual (hoy, semana, mes,
        // anio) o null si es personalizado. Sirve para resaltar el boton correcto sin importar como
        // se envio el formulario (por ejemplo, tras cambiar otro filtro sobre "Este mes").
        public static string? RangoActivo(DateTime? desde, DateTime? hasta)
        {
            if (!desde.HasValue || !hasta.HasValue) return null;
            var hoy = DateTime.Today;
            if (hasta.Value.Date != hoy) return null; // todos los accesos rapidos terminan hoy
            var d = desde.Value.Date;
            if (d == hoy) return "hoy";
            if (d == hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7))) return "semana";
            if (d == new DateTime(hoy.Year, hoy.Month, 1)) return "mes";
            if (d == new DateTime(hoy.Year, 1, 1)) return "anio";
            return null;
        }
    }
}
