namespace CasaVintage.ViewModels
{
    // Key business numbers for the report cards.
    public sealed record ReporteResumenViewModel(
        int TotalVentas,
        int UnidadesVendidas,
        decimal Ingresos,
        decimal Ganancia,
        decimal TicketPromedio);

    // A product with how much of it has been sold (best/least-selling).
    public sealed record ProductoVendidoViewModel(
        string Sku,
        string Nombre,
        int CantidadVendida,
        decimal Ingresos);

    // Sales grouped by salesperson.
    public sealed record VentaPorVendedorViewModel(
        string Vendedor,
        int NumeroVentas,
        decimal Total);

    // A row of the sales history.
    public sealed record VentaHistorialViewModel(
        int IdVenta,
        DateTime Fecha,
        string Cliente,
        string Vendedor,
        string MetodoPago,
        int Articulos,
        decimal Total);

    // A salesperson for the filter dropdown (user id + name).
    public sealed record VendedorOpcionViewModel(int Id, string Nombre);

    // Filter for the Accountant's reports. Applied INSIDE the EF Core queries (not in memory):
    // date range on ventas.fecha, salesperson by ventas.id_usuario, payment method and product
    // category. Any combination is optional.
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

        // Builds the filter from the request parameters. "rango" is the quick access
        // ("hoy", "semana", "mes", "anio"); if it is not one of them, the desde/hasta dates are used.
        public static ReporteFiltro Construir(string? rango, DateTime? desde, DateTime? hasta,
            int? vendedor, string? metodo, string? categoria)
        {
            var hoy = DateTime.Today;
            switch (rango)
            {
                case "hoy": desde = hoy; hasta = hoy; break;
                case "semana": desde = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7)); hasta = hoy; break; // from Monday
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

        // Returns the quick access that matches the current date range (hoy, semana, mes, anio) or
        // null if it is custom. Used to highlight the correct button regardless of how the form was
        // submitted (for example, after changing another filter over "This month").
        public static string? RangoActivo(DateTime? desde, DateTime? hasta)
        {
            if (!desde.HasValue || !hasta.HasValue) return null;
            var hoy = DateTime.Today;
            if (hasta.Value.Date != hoy) return null; // all quick accesses end today
            var d = desde.Value.Date;
            if (d == hoy) return "hoy";
            if (d == hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7))) return "semana";
            if (d == new DateTime(hoy.Year, hoy.Month, 1)) return "mes";
            if (d == new DateTime(hoy.Year, 1, 1)) return "anio";
            return null;
        }
    }
}
