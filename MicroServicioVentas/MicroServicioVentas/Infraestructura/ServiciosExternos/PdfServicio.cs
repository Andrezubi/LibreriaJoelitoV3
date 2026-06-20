using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MicroServicioVentas.Infraestructura.ServiciosExternos
{
    public class PdfServicio : IPdfServicio
    {
        private readonly IWebHostEnvironment _env;

        public PdfServicio(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerarComprobanteVenta(VentaCompletaDTO ventaCompleta)
        {
            if (ventaCompleta == null || ventaCompleta.Detalles == null || ventaCompleta.Detalles.Count == 0)
                return Array.Empty<byte>();

            var venta = ventaCompleta.Venta;
            var detalles = ventaCompleta.Detalles;

            string rutaLogo = Path.Combine(_env.ContentRootPath, "Recursos", "Imagenes", "logo-lib.png");

            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(1.5f, Unit.Centimetre);
                    pagina.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(80).Height(70).Element(e =>
                            {
                                if (File.Exists(rutaLogo))
                                    e.Image(rutaLogo).FitArea();
                                else
                                    e.Border(1).AlignCenter().AlignMiddle().Text("LOGO").FontSize(10);
                            });

                            row.RelativeItem().PaddingLeft(15).Column(info =>
                            {
                                info.Item().Text("LIBRERÍA JOELITO").FontSize(20).Bold();
                                info.Item().Text("Comprobante de venta").FontSize(14);
                                info.Item().Text($"Venta Nro. {venta.Id}").FontSize(10);
                                info.Item().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}").FontSize(10);
                            });
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1);
                    });

                    pagina.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("Datos del cliente").FontSize(13).Bold();

                        col.Item().PaddingTop(5).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120);
                                columns.RelativeColumn();
                            });

                            AgregarFila(tabla, "Razón social:", venta.RazonSocialCliente);
                            AgregarFila(tabla, "CI:", venta.CiCompleto);
                            AgregarFila(tabla, "Email:", venta.EmailCliente ?? "-");
                            AgregarFila(tabla, "Cliente frecuente:", venta.ClienteFrecuente ? "Sí" : "No");
                            AgregarFila(tabla, "Usuario:", $"Usuario {venta.IdUsuario}");
                            AgregarFila(tabla, "Estado:", venta.EstadoVenta);
                        });

                        col.Item().PaddingTop(20).Text("Detalle de productos").FontSize(13).Bold();

                        col.Item().PaddingTop(5).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(55);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            tabla.Header(header =>
                            {
                                AgregarCabecera(header, "Producto");
                                AgregarCabecera(header, "Presentación");
                                AgregarCabecera(header, "Cant.");
                                AgregarCabecera(header, "P. Unit.");
                                AgregarCabecera(header, "Subtotal");
                            });

                            foreach (var detalle in detalles)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(detalle.Producto);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(detalle.Presentacion);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(detalle.Cantidad.ToString());
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{detalle.PrecioUnitario:0.00}");
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{detalle.Subtotal:0.00}");
                            }
                        });

                        col.Item().PaddingTop(15).AlignRight().Text($"TOTAL: Bs {venta.Total:0.00}")
                            .FontSize(14)
                            .Bold();

                        col.Item().PaddingTop(20).Text($"CorrelationId: {venta.CorrelationId}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    pagina.Footer().AlignCenter().Text(texto =>
                    {
                        texto.Span("Gracias por su compra - Librería Joelito").FontSize(9);
                    });
                });
            }).GeneratePdf();
        }

        private static void AgregarFila(TableDescriptor tabla, string etiqueta, string valor)
        {
            tabla.Cell().PaddingVertical(2).Text(etiqueta).SemiBold();
            tabla.Cell().PaddingVertical(2).Text(valor);
        }

        private static void AgregarCabecera(TableCellDescriptor celda, string texto)
        {
            celda.Background(Colors.Grey.Lighten3)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(5)
                .Text(texto)
                .Bold();
        }
    }
}