using CasaVintage.Data;
using CasaVintage.Models;
using CasaVintage.ViewModels;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CasaVintage.Services
{
    // Reports implementation. Simple rows are fetched from the database and aggregated in memory: EF
    // does not translate GroupBy with navigation + calculation to SQL, and the store's sales volume
    // is small. Profit uses each line's sale price minus the product's current cost.
    public class ReporteService : IReporteService
    {
        // Vintage (autumnal) palette for the exported documents.
        private const string Cafe = "#6D3F1C";       // main brown (Cowhide Cocoa)
        private const string Crema = "#F6EFE1";       // cream background
        private const string CremaTarjeta = "#ECE3CD";// cards / soft bands
        private const string Caramelo = "#8A6A2B";    // Toasted Caramel (accent)
        private const string Oliva = "#9D9167";       // Olive Harvest
        private const string Tinta = "#33281B";       // main text
        private const string TintaSuave = "#7C6C52";  // secondary text
        private const string Borde = "#E7DCC4";       // soft borders

        private readonly CasaVintageContext _db;
        private readonly IMarcaService _marca;

        public ReporteService(CasaVintageContext db, IMarcaService marca)
        {
            _db = db;
            _marca = marca;
        }

        public async Task<ReporteResumenViewModel> ObtenerResumenAsync(ReporteFiltro filtro)
        {
            // The filter is applied in the query (SQL); the aggregation is done over the already-filtered
            // rows. It is computed from the lines so the category filters consistently; without a
            // category, the total matches the sum of total_pagado of the sales.
            var lineas = await AplicarFiltroDetalle(_db.DetallesVenta.AsNoTracking(), filtro)
                .Select(d => new { d.IdVenta, d.Cantidad, d.PrecioUnitario, Costo = d.Producto!.Costo })
                .ToListAsync();

            var totalVentas = lineas.Select(l => l.IdVenta).Distinct().Count();
            var unidades = lineas.Sum(l => l.Cantidad);
            var ingresos = lineas.Sum(l => l.PrecioUnitario * l.Cantidad);
            var ganancia = lineas.Sum(l => (l.PrecioUnitario - l.Costo) * l.Cantidad);
            var ticket = totalVentas > 0 ? ingresos / totalVentas : 0m;

            return new ReporteResumenViewModel(totalVentas, unidades, ingresos, ganancia, ticket);
        }

        public async Task<IReadOnlyList<ProductoVendidoViewModel>> MasVendidosAsync(ReporteFiltro filtro, int top = 5)
        {
            var vendidos = await VendidosPorProductoAsync(filtro);
            return vendidos
                .OrderByDescending(p => p.CantidadVendida)
                .ThenByDescending(p => p.Ingresos)
                .Take(top)
                .ToList();
        }

        public async Task<IReadOnlyList<ProductoVendidoViewModel>> MenosVendidosAsync(ReporteFiltro filtro, int top = 5)
        {
            var vendidos = await VendidosPorProductoAsync(filtro);
            return vendidos
                .OrderBy(p => p.CantidadVendida)
                .ThenBy(p => p.Nombre)
                .Take(top)
                .ToList();
        }

        // Products with how much of each has been sold (includes the ones at 0). With a category in the
        // filter, only the products of that category; sales are limited by date/salesperson/method.
        private async Task<List<ProductoVendidoViewModel>> VendidosPorProductoAsync(ReporteFiltro filtro)
        {
            var productosConsulta = _db.Productos.AsNoTracking();
            if (!string.IsNullOrEmpty(filtro.Categoria))
            {
                productosConsulta = productosConsulta.Where(p => p.Categoria == filtro.Categoria);
            }
            var productos = await productosConsulta
                .Select(p => new { p.IdProducto, p.Sku, p.Nombre })
                .ToListAsync();

            var detalles = await AplicarFiltroDetalle(_db.DetallesVenta.AsNoTracking(), filtro)
                .Select(d => new { d.IdProducto, d.Cantidad, d.PrecioUnitario })
                .ToListAsync();

            var porProducto = detalles
                .GroupBy(d => d.IdProducto)
                .ToDictionary(
                    g => g.Key,
                    g => new { Cantidad = g.Sum(x => x.Cantidad), Ingresos = g.Sum(x => x.PrecioUnitario * x.Cantidad) });

            return productos
                .Select(p =>
                {
                    porProducto.TryGetValue(p.IdProducto, out var v);
                    return new ProductoVendidoViewModel(p.Sku, p.Nombre, v?.Cantidad ?? 0, v?.Ingresos ?? 0m);
                })
                .ToList();
        }

        public async Task<IReadOnlyList<VentaPorVendedorViewModel>> VentasPorVendedorAsync(ReporteFiltro filtro)
        {
            // From the filtered lines: per salesperson, number of distinct sales and total sold.
            var lineas = await AplicarFiltroDetalle(_db.DetallesVenta.AsNoTracking(), filtro)
                .Select(d => new { Vendedor = d.Venta!.Usuario!.NombreUsuario, d.IdVenta, d.PrecioUnitario, d.Cantidad })
                .ToListAsync();

            return lineas
                .GroupBy(l => l.Vendedor)
                .Select(g => new VentaPorVendedorViewModel(
                    g.Key,
                    g.Select(x => x.IdVenta).Distinct().Count(),
                    g.Sum(x => x.PrecioUnitario * x.Cantidad)))
                .OrderByDescending(v => v.Total)
                .ToList();
        }

        public async Task<IReadOnlyList<VentaHistorialViewModel>> HistorialAsync(ReporteFiltro filtro)
        {
            var ventas = await AplicarFiltroVenta(_db.Ventas.AsNoTracking(), filtro)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new
                {
                    v.IdVenta,
                    v.Fecha,
                    Cliente = v.Cliente!.Nombre,
                    Vendedor = v.Usuario!.NombreUsuario,
                    v.MetodoPago,
                    v.TotalPagado,
                    Articulos = v.Detalles.Sum(d => (int?)d.Cantidad) ?? 0
                })
                .ToListAsync();

            return ventas
                .Select(v => new VentaHistorialViewModel(
                    v.IdVenta, v.Fecha, v.Cliente, v.Vendedor, v.MetodoPago, v.Articulos, v.TotalPagado))
                .ToList();
        }

        public async Task<IReadOnlyList<VendedorOpcionViewModel>> VendedoresAsync()
        {
            return await _db.Usuarios
                .AsNoTracking()
                .Where(u => u.Rol == "Vendedor")
                .OrderBy(u => u.NombreUsuario)
                .Select(u => new VendedorOpcionViewModel(u.IdUsuario, u.NombreUsuario))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> CategoriasAsync()
        {
            return await _db.Productos
                .AsNoTracking()
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        // ---- Applying the filter INSIDE the query (translated to SQL) ----
        private static IQueryable<Venta> AplicarFiltroVenta(IQueryable<Venta> q, ReporteFiltro f)
        {
            if (f.Desde.HasValue) { var d = f.Desde.Value.Date; q = q.Where(v => v.Fecha >= d); }
            if (f.Hasta.HasValue) { var h = f.Hasta.Value.Date.AddDays(1); q = q.Where(v => v.Fecha < h); }
            if (f.IdVendedor.HasValue) q = q.Where(v => v.IdUsuario == f.IdVendedor.Value);
            if (!string.IsNullOrEmpty(f.MetodoPago)) q = q.Where(v => v.MetodoPago == f.MetodoPago);
            if (!string.IsNullOrEmpty(f.Categoria)) q = q.Where(v => v.Detalles.Any(d => d.Producto!.Categoria == f.Categoria));
            return q;
        }

        private static IQueryable<DetalleVenta> AplicarFiltroDetalle(IQueryable<DetalleVenta> q, ReporteFiltro f)
        {
            if (f.Desde.HasValue) { var d = f.Desde.Value.Date; q = q.Where(x => x.Venta!.Fecha >= d); }
            if (f.Hasta.HasValue) { var h = f.Hasta.Value.Date.AddDays(1); q = q.Where(x => x.Venta!.Fecha < h); }
            if (f.IdVendedor.HasValue) q = q.Where(x => x.Venta!.IdUsuario == f.IdVendedor.Value);
            if (!string.IsNullOrEmpty(f.MetodoPago)) q = q.Where(x => x.Venta!.MetodoPago == f.MetodoPago);
            if (!string.IsNullOrEmpty(f.Categoria)) q = q.Where(x => x.Producto!.Categoria == f.Categoria);
            return q;
        }

        // Payment method display text (the stored value stays in Spanish; this only affects display).
        private static string MetodoPagoTexto(string metodo) => metodo switch
        {
            "Efectivo" => "Cash",
            "Tarjeta" => "Card",
            _ => metodo
        };

        // Text describing the active filter (subtitle of the exported documents).
        private async Task<string> DescripcionFiltroAsync(ReporteFiltro f)
        {
            if (!f.HayFiltro)
            {
                return "All data";
            }
            var partes = new List<string>();
            if (f.Desde.HasValue || f.Hasta.HasValue)
            {
                var desde = f.Desde?.ToString("dd/MM/yyyy") ?? "start";
                var hasta = f.Hasta?.ToString("dd/MM/yyyy") ?? "today";
                partes.Add($"Period {desde} - {hasta}");
            }
            if (f.IdVendedor.HasValue)
            {
                var nombre = await _db.Usuarios.Where(u => u.IdUsuario == f.IdVendedor.Value)
                    .Select(u => u.NombreUsuario).FirstOrDefaultAsync();
                partes.Add($"Salesperson: {nombre}");
            }
            if (!string.IsNullOrEmpty(f.MetodoPago)) partes.Add($"Payment: {MetodoPagoTexto(f.MetodoPago)}");
            if (!string.IsNullOrEmpty(f.Categoria)) partes.Add($"Category: {f.Categoria}");
            return string.Join("   |   ", partes);
        }

        public async Task<byte[]> GenerarPdfAsync(ReporteFiltro filtro)
        {
            var resumen = await ObtenerResumenAsync(filtro);
            var productos = await MasVendidosAsync(filtro, 1000);
            var vendedores = await VentasPorVendedorAsync(filtro);
            var descripcion = await DescripcionFiltroAsync(filtro);
            var logo = _marca.LogoBytes();

            var documento = Document.Create(contenedor =>
            {
                contenedor.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.4f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontSize(10).FontColor(Tinta));

                    page.Header().Row(fila =>
                    {
                        if (logo is not null)
                        {
                            // FitArea inside a fixed box: keeps the proportion and never clashes
                            // (works with landscape, square or portrait logos).
                            fila.ConstantItem(90).Height(56).AlignLeft().AlignMiddle().Image(logo).FitArea();
                            fila.ConstantItem(12);
                        }
                        fila.RelativeItem().AlignMiddle().Column(col =>
                        {
                            col.Item().Text("The Vintage House").FontSize(18).Bold().FontColor(Cafe);
                            col.Item().Text("General sales report").FontSize(11).FontColor(TintaSuave);
                            col.Item().Text(descripcion).FontSize(8).FontColor(TintaSuave);
                        });
                    });

                    page.Content().PaddingVertical(16).Column(col =>
                    {
                        col.Item().PaddingBottom(8).Text("Summary").FontSize(13).Bold();
                        col.Item().Row(r =>
                        {
                            ResumenCelda(r, "Sales", resumen.TotalVentas.ToString());
                            ResumenCelda(r, "Units", resumen.UnidadesVendidas.ToString());
                            ResumenCelda(r, "Revenue", $"${resumen.Ingresos:N2}");
                            ResumenCelda(r, "Profit", $"${resumen.Ganancia:N2}");
                        });

                        col.Item().PaddingTop(16).PaddingBottom(6).Text("Best-selling products").FontSize(13).Bold().FontColor(Cafe);
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); c.RelativeColumn(1.5f); });
                            tabla.Header(h =>
                            {
                                h.Cell().Background(Cafe).Padding(5).Text("Product").Bold().FontColor(Colors.White);
                                h.Cell().Background(Cafe).Padding(5).AlignRight().Text("Sold").Bold().FontColor(Colors.White);
                                h.Cell().Background(Cafe).Padding(5).AlignRight().Text("Revenue").Bold().FontColor(Colors.White);
                            });
                            foreach (var p in productos)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).Text($"{p.Nombre} ({p.Sku})");
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(p.CantidadVendida.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text($"${p.Ingresos:N2}");
                            }
                        });

                        col.Item().PaddingTop(16).PaddingBottom(6).Text("Sales by salesperson").FontSize(13).Bold().FontColor(Cafe);
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); c.RelativeColumn(1.5f); });
                            tabla.Header(h =>
                            {
                                h.Cell().Background(Cafe).Padding(5).Text("Salesperson").Bold().FontColor(Colors.White);
                                h.Cell().Background(Cafe).Padding(5).AlignRight().Text("Sales").Bold().FontColor(Colors.White);
                                h.Cell().Background(Cafe).Padding(5).AlignRight().Text("Total").Bold().FontColor(Colors.White);
                            });
                            foreach (var v in vendedores)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).Text(v.Vendedor);
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(v.NumeroVentas.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text($"${v.Total:N2}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text("The Vintage House - Sales report").FontSize(8).FontColor(TintaSuave);
                });
            });

            return documento.GeneratePdf();
        }

        private static void ResumenCelda(RowDescriptor fila, string etiqueta, string valor)
        {
            fila.RelativeItem().PaddingRight(6).Background(CremaTarjeta).Border(1).BorderColor(Borde).Padding(10).Column(c =>
            {
                c.Item().Text(etiqueta.ToUpperInvariant()).FontSize(8).FontColor(TintaSuave);
                c.Item().PaddingTop(2).Text(valor).FontSize(15).Bold().FontColor(Cafe);
            });
        }

        public async Task<byte[]> GenerarExcelAsync(ReporteFiltro filtro)
        {
            var resumen = await ObtenerResumenAsync(filtro);
            var productos = await MasVendidosAsync(filtro, 1000);
            var vendedores = await VentasPorVendedorAsync(filtro);
            var descripcion = await DescripcionFiltroAsync(filtro);
            var logo = _marca.LogoBytes();

            // Vintage palette for the workbook.
            var cafe = XLColor.FromHtml(Cafe);
            var crema = XLColor.FromHtml(Crema);
            var cremaCard = XLColor.FromHtml("#F0E7D4");
            var caramelo = XLColor.FromHtml(Caramelo);
            var oliva = XLColor.FromHtml(Oliva);
            var tinta = XLColor.FromHtml(Tinta);
            var tintaSuave = XLColor.FromHtml(TintaSuave);
            var bordeCol = XLColor.FromHtml(Borde);
            const string Moneda = "$#,##0.00";

            // Table header: brown background, white bold text.
            static void EncabezadoTabla(IXLRange rango, XLColor fondo)
            {
                rango.Style.Font.Bold = true;
                rango.Style.Font.FontColor = XLColor.White;
                rango.Style.Fill.BackgroundColor = fondo;
                rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            using var libro = new XLWorkbook();

            // ================= Summary sheet (dashboard) =================
            var r = libro.Worksheets.Add("Summary");
            r.ShowGridLines = false;
            r.Style.Font.FontName = "Segoe UI";

            // Column widths (A = margin; B..I = content, 2 columns per card).
            r.Column(1).Width = 3;
            for (var c = 2; c <= 9; c++)
            {
                r.Column(c).Width = 15;
            }

            // General cream background.
            r.Range(1, 1, 40, 9).Style.Fill.BackgroundColor = crema;

            // Title and subtitle (centered across the content width).
            var titulo = r.Range(2, 2, 2, 9).Merge();
            titulo.Value = "GENERAL REPORT";
            titulo.Style.Font.FontName = "Georgia";
            titulo.Style.Font.FontSize = 24;
            titulo.Style.Font.Bold = true;
            titulo.Style.Font.FontColor = cafe;
            titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var sub = r.Range(3, 2, 3, 9).Merge();
            sub.Value = $"Generated on {DateTime.Now:dd/MM/yyyy HH:mm}   ·   {descripcion}";
            sub.Style.Font.FontColor = tintaSuave;
            sub.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            r.Row(2).Height = 34;

            // Logo (top-left corner), if any. Keeps its proportion.
            if (logo is not null)
            {
                // The stream is kept alive until the workbook is saved (not disposed early with 'using').
                var ms = new MemoryStream(logo);
                var pic = r.AddPicture(ms, "logo").MoveTo(r.Cell(2, 2));
                if (pic.OriginalHeight > 0)
                {
                    pic.Scale(60.0 / pic.OriginalHeight);
                }
            }

            // KPI cards (row 5 label, 6-7 value, 8 subtitle). Each one spans 2 columns.
            void Tarjeta(int col, string etiqueta, string valor, string subtitulo, XLColor valorColor)
            {
                var card = r.Range(5, col, 8, col + 1);
                card.Style.Fill.BackgroundColor = cremaCard;
                card.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                card.Style.Border.OutsideBorderColor = bordeCol;

                var eti = r.Range(5, col, 5, col + 1).Merge();
                eti.Value = etiqueta;
                eti.Style.Font.Bold = true;
                eti.Style.Font.FontSize = 9;
                eti.Style.Font.FontColor = tintaSuave;
                eti.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                eti.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var val = r.Range(6, col, 7, col + 1).Merge();
                val.Value = valor;
                val.Style.Font.Bold = true;
                val.Style.Font.FontSize = 20;
                val.Style.Font.FontColor = valorColor;
                val.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                val.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var s = r.Range(8, col, 8, col + 1).Merge();
                s.Value = subtitulo;
                s.Style.Font.FontSize = 8;
                s.Style.Font.FontColor = tintaSuave;
                s.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            r.Row(6).Height = 20;
            r.Row(7).Height = 20;
            Tarjeta(2, "REGISTERED SALES", resumen.TotalVentas.ToString(), $"{resumen.UnidadesVendidas} units sold", cafe);
            Tarjeta(4, "TOTAL REVENUE", $"${resumen.Ingresos:N2}", "Total of sales", cafe);
            Tarjeta(6, "PROFIT (PROFITABILITY)", $"${resumen.Ganancia:N2}", "Price minus cost", oliva);
            Tarjeta(8, "AVERAGE TICKET", $"${resumen.TicketPromedio:N2}", "Average per sale", caramelo);

            // ---- Table: Best-selling products ----
            var f = 10;
            var tp = r.Range(f, 2, f, 5).Merge();
            tp.Value = "BEST-SELLING PRODUCTS";
            tp.Style.Font.Bold = true;
            tp.Style.Font.FontColor = cafe;
            f++;
            r.Cell(f, 2).Value = "SKU";
            r.Cell(f, 3).Value = "Product";
            r.Cell(f, 4).Value = "Sold";
            r.Cell(f, 5).Value = "Revenue";
            EncabezadoTabla(r.Range(f, 2, f, 5), cafe);
            f++;
            foreach (var p in productos)
            {
                r.Cell(f, 2).Value = p.Sku;
                r.Cell(f, 3).Value = p.Nombre;
                r.Cell(f, 4).Value = p.CantidadVendida;
                r.Cell(f, 5).Value = p.Ingresos;
                r.Cell(f, 5).Style.NumberFormat.Format = Moneda;
                r.Range(f, 2, f, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                r.Range(f, 2, f, 5).Style.Border.BottomBorderColor = bordeCol;
                f++;
            }

            // ---- Table: Sales by salesperson ----
            f += 1;
            var tv = r.Range(f, 2, f, 5).Merge();
            tv.Value = "SALES BY SALESPERSON";
            tv.Style.Font.Bold = true;
            tv.Style.Font.FontColor = cafe;
            f++;
            r.Cell(f, 2).Value = "Salesperson";
            r.Cell(f, 4).Value = "Sales";
            r.Cell(f, 5).Value = "Total";
            r.Range(f, 2, f, 3).Merge();
            EncabezadoTabla(r.Range(f, 2, f, 5), cafe);
            f++;
            foreach (var v in vendedores)
            {
                r.Range(f, 2, f, 3).Merge();
                r.Cell(f, 2).Value = v.Vendedor;
                r.Cell(f, 4).Value = v.NumeroVentas;
                r.Cell(f, 5).Value = v.Total;
                r.Cell(f, 5).Style.NumberFormat.Format = Moneda;
                r.Range(f, 2, f, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                r.Range(f, 2, f, 5).Style.Border.BottomBorderColor = bordeCol;
                f++;
            }
            // TOTAL row.
            r.Range(f, 2, f, 3).Merge();
            r.Cell(f, 2).Value = "TOTAL";
            r.Cell(f, 4).Value = vendedores.Sum(v => v.NumeroVentas);
            r.Cell(f, 5).Value = vendedores.Sum(v => v.Total);
            r.Cell(f, 5).Style.NumberFormat.Format = Moneda;
            r.Range(f, 2, f, 5).Style.Font.Bold = true;
            r.Range(f, 2, f, 5).Style.Fill.BackgroundColor = cremaCard;

            // ================= Products sheet (detail) =================
            var hp = libro.Worksheets.Add("Products");
            hp.Cell(1, 1).Value = "SKU";
            hp.Cell(1, 2).Value = "Product";
            hp.Cell(1, 3).Value = "Sold";
            hp.Cell(1, 4).Value = "Revenue";
            EncabezadoTabla(hp.Range(1, 1, 1, 4), cafe);
            var fila = 2;
            foreach (var p in productos)
            {
                hp.Cell(fila, 1).Value = p.Sku;
                hp.Cell(fila, 2).Value = p.Nombre;
                hp.Cell(fila, 3).Value = p.CantidadVendida;
                hp.Cell(fila, 4).Value = p.Ingresos;
                hp.Cell(fila, 4).Style.NumberFormat.Format = Moneda;
                fila++;
            }
            hp.Columns().AdjustToContents();
            hp.SheetView.FreezeRows(1);

            // ================= Salespeople sheet (detail) =================
            var hv = libro.Worksheets.Add("Salespeople");
            hv.Cell(1, 1).Value = "Salesperson";
            hv.Cell(1, 2).Value = "Sales";
            hv.Cell(1, 3).Value = "Total";
            EncabezadoTabla(hv.Range(1, 1, 1, 3), cafe);
            fila = 2;
            foreach (var v in vendedores)
            {
                hv.Cell(fila, 1).Value = v.Vendedor;
                hv.Cell(fila, 2).Value = v.NumeroVentas;
                hv.Cell(fila, 3).Value = v.Total;
                hv.Cell(fila, 3).Style.NumberFormat.Format = Moneda;
                fila++;
            }
            hv.Columns().AdjustToContents();
            hv.SheetView.FreezeRows(1);

            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);
            return memoria.ToArray();
        }
    }
}
