using System.Globalization;
using CasaVintage.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CasaVintage.Services
{
    // Genera el comprobante de una venta: arma el ViewModel (con desglose de IVA y datos de la
    // empresa desde appsettings), dibuja el PDF con QuestPDF y lo envia por correo con IEmailService.
    public class FacturaService : IFacturaService
    {
        // Colores de la paleta vintage (otonal) usados en el PDF.
        private const string Cafe = "#6D3F1C";      // marron principal (Cowhide Cocoa)
        private const string Crema = "#ECE3CD";      // crema clara
        private const string Caramelo = "#8A6A2B";   // Toasted Caramel (acentos)
        private const string Tinta = "#33281B";      // texto principal
        private const string TintaSuave = "#7C6C52"; // texto secundario
        private const string Borde = "#E7DCC4";      // bordes suaves

        private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-SV");

        private readonly IVentaService _ventas;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly IArchivadorLocal _archivador;
        private readonly IMarcaService _marca;

        public FacturaService(IVentaService ventas, IEmailService email, IConfiguration config, IArchivadorLocal archivador, IMarcaService marca)
        {
            _ventas = ventas;
            _email = email;
            _config = config;
            _archivador = archivador;
            _marca = marca;
        }

        public async Task<FacturaViewModel?> ObtenerFacturaAsync(int idVenta)
        {
            var venta = await _ventas.ObtenerConfirmacionAsync(idVenta);
            if (venta is null)
            {
                return null;
            }

            // Precios con IVA incluido: el subtotal y el IVA se extraen del total (no cambia lo pagado).
            var subtotal = Math.Round(venta.Total / (1 + FacturaViewModel.TasaIva), 2, MidpointRounding.AwayFromZero);
            var iva = venta.Total - subtotal;

            var empresa = _config.GetSection("Empresa");
            return new FacturaViewModel(
                venta.IdVenta,
                venta.Fecha,
                empresa["Nombre"] ?? "La Casa de Vintage",
                empresa["Direccion"] ?? "",
                empresa["Telefono"] ?? "",
                empresa["Correo"] ?? "",
                venta.ClienteNombre,
                venta.ClienteCorreo,
                venta.MetodoPago,
                venta.VendedorNombre,
                venta.Lineas,
                subtotal,
                iva,
                venta.Total,
                venta.TarjetaUltimos4);
        }

        public async Task<byte[]?> GenerarPdfAsync(int idVenta)
        {
            var factura = await ObtenerFacturaAsync(idVenta);
            return factura is null ? null : ConstruirPdf(factura);
        }

        // Logo de la empresa (o null) para incrustarlo en el comprobante.
        private byte[]? Logo() => _marca.LogoBytes();

        public async Task<ResultadoEnvio> EnviarPorCorreoAsync(int idVenta)
        {
            var factura = await ObtenerFacturaAsync(idVenta);
            if (factura is null)
            {
                return new ResultadoEnvio(false, null, "La venta no existe.");
            }
            if (string.IsNullOrWhiteSpace(factura.ClienteCorreo))
            {
                return new ResultadoEnvio(false, null, "La venta no tiene correo del cliente.");
            }

            var pdf = ConstruirPdf(factura);
            var cuerpo =
                $"<p>Hola {factura.ClienteNombre},</p>" +
                $"<p>Adjuntamos el comprobante de tu compra en La Casa de Vintage (venta #{factura.NumeroVenta:D7}).</p>" +
                "<p>Gracias por tu compra.</p>";

            var enviado = await _email.EnviarConAdjuntoAsync(
                factura.ClienteCorreo,
                $"Comprobante de compra #{factura.NumeroVenta:D7} - La Casa de Vintage",
                cuerpo,
                pdf,
                $"factura-{factura.NumeroVenta:D7}.pdf");

            return enviado
                ? new ResultadoEnvio(true, factura.ClienteCorreo)
                : new ResultadoEnvio(false, factura.ClienteCorreo, "No se pudo enviar el correo. Revisa la configuracion SMTP.");
        }

        public async Task GuardarEnCarpetaAsync(int idVenta)
        {
            var pdf = await GenerarPdfAsync(idVenta);
            if (pdf is null)
            {
                return;
            }
            await _archivador.GuardarAsync("Facturas", $"factura-{idVenta:D7}.pdf", pdf);
        }

        // ---- Dibujo del PDF ----
        private byte[] ConstruirPdf(FacturaViewModel f)
        {
            var logo = Logo();
            var documento = Document.Create(contenedor =>
            {
                contenedor.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.4f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontSize(10).FontColor(Tinta));

                    page.Header().Element(e => Encabezado(e, f, logo));
                    page.Content().Element(e => Cuerpo(e, f));
                    page.Footer().AlignCenter().Text("La Casa de Vintage - Comprobante interno").FontSize(8).FontColor(TintaSuave);
                });
            });

            return documento.GeneratePdf();
        }

        private static void Encabezado(IContainer contenedor, FacturaViewModel f, byte[]? logo)
        {
            contenedor.Row(fila =>
            {
                fila.ConstantItem(160).Background(Cafe).Padding(16).Column(col =>
                {
                    if (logo is not null)
                    {
                        // Fondo blanco para que un logo con transparencia se lea sobre el cafe.
                        // FitArea en caja fija: conserva proporcion y no choca con cualquier logo.
                        col.Item().Background(Colors.White).Padding(6).Height(58).AlignCenter().Image(logo).FitArea();
                        col.Item().PaddingTop(8).Text("LA CASA DE VINTAGE").FontSize(10).Bold().FontColor(Colors.White);
                    }
                    else
                    {
                        col.Item().Text("V").FontSize(28).Bold().FontColor(Colors.White);
                        col.Item().PaddingTop(4).Text("LA CASA DE VINTAGE").FontSize(11).Bold().FontColor(Colors.White);
                    }
                    col.Item().Text("Tienda de antigüedades").FontSize(8).FontColor(Crema);
                });

                fila.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"FACTURA N° {f.NumeroVenta:D7}").Bold().FontSize(13).FontColor(Cafe);
                    col.Item().PaddingTop(2).Text($"Fecha: {f.Fecha.ToString("dd 'de' MMMM yyyy", Es)}").FontSize(9).FontColor(TintaSuave);
                    col.Item().Text($"Metodo de pago: {f.MetodoPagoTexto}").FontSize(9).FontColor(TintaSuave);
                });
            });
        }

        private static void Cuerpo(IContainer contenedor, FacturaViewModel f)
        {
            contenedor.PaddingVertical(18).Column(col =>
            {
                // Empresa
                col.Item().Text(f.EmpresaNombre).FontSize(18).Bold().FontColor(Cafe);
                col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Caramelo);
                col.Item().Text(f.EmpresaTelefono).FontColor(TintaSuave);
                col.Item().Text(f.EmpresaDireccion).FontColor(TintaSuave);
                col.Item().Text(f.EmpresaCorreo).FontColor(TintaSuave);

                // Cliente
                col.Item().PaddingTop(16).Text(t =>
                {
                    t.Span("Cliente: ").Bold();
                    t.Span(f.ClienteNombre);
                });
                col.Item().Text(t =>
                {
                    t.Span("Correo: ").Bold();
                    t.Span(string.IsNullOrEmpty(f.ClienteCorreo) ? "-" : f.ClienteCorreo);
                });
                col.Item().Text(t =>
                {
                    t.Span("Atendió: ").Bold();
                    t.Span(f.VendedorNombre);
                });

                // Tabla de conceptos
                col.Item().PaddingTop(18).Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(1.4f);
                    });

                    tabla.Header(h =>
                    {
                        h.Cell().BorderBottom(1).BorderColor(Tinta).PaddingBottom(6).Text("CONCEPTO").Bold().FontSize(9);
                        h.Cell().BorderBottom(1).BorderColor(Tinta).PaddingBottom(6).AlignRight().Text("CANTIDAD").Bold().FontSize(9);
                        h.Cell().BorderBottom(1).BorderColor(Tinta).PaddingBottom(6).AlignRight().Text("PRECIO").Bold().FontSize(9);
                        h.Cell().BorderBottom(1).BorderColor(Tinta).PaddingBottom(6).AlignRight().Text("TOTAL").Bold().FontSize(9);
                    });

                    foreach (var linea in f.Lineas)
                    {
                        tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(8).Text(linea.Nombre);
                        tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(8).AlignRight().Text(linea.Cantidad.ToString());
                        tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(8).AlignRight().Text($"${linea.PrecioUnitario:N2}");
                        tabla.Cell().BorderBottom(1).BorderColor(Borde).PaddingVertical(8).AlignRight().Text($"${linea.Subtotal:N2}");
                    }
                });

                // Totales (alineados a la derecha)
                col.Item().PaddingTop(12).AlignRight().Width(240).Column(tot =>
                {
                    tot.Item().Row(r =>
                    {
                        r.RelativeItem().Text("SUBTOTAL").Bold();
                        r.ConstantItem(100).AlignRight().Text($"${f.Subtotal:N2}");
                    });
                    tot.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text("IVA (13%)").Bold();
                        r.ConstantItem(100).AlignRight().Text($"${f.Iva:N2}");
                    });
                    tot.Item().PaddingTop(8).BorderTop(1).BorderColor(Cafe).PaddingTop(8).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL").Bold().FontSize(13).FontColor(Cafe);
                        r.ConstantItem(100).AlignRight().Text($"${f.Total:N2}").Bold().FontSize(13).FontColor(Cafe);
                    });
                });
            });
        }
    }
}
