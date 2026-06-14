using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;
using System.IO;

namespace MicroServicioVentas.Infraestructura.ServiciosExternos
{
    public class PdfServicio : IPdfServicio
    {

        private readonly IWebHostEnvironment _env;

        public PdfServicio(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerarComprobanteVenta(DataTable dt)
        {
            if (dt.Rows.Count == 0) return Array.Empty<byte>();

            DataRow cabecera = dt.Rows[0];
            decimal total = Convert.ToDecimal(cabecera["Total"]);

            string rutaLogo = Path.Combine(_env.ContentRootPath, "Recursos", "Imagenes", "logo-lib.png");
            string nombreEmpleado = cabecera["NombreEmpleado"].ToString() ?? string.Empty;
            string razonSocial = cabecera["RS"].ToString() ?? string.Empty;

            string colorPrincipal = "#7B2CBF";
            string colorSecundario = "#9D4EDD";
            string colorFondo = "#F3E8FF";
            string colorBorde = "#D8B4FE";
            string colorTexto = "#1F1235";
            string colorMoradoClaro = "#E9D5FF";
            string colorMoradoMuyClaro = "#FAF5FF";
            string colorMoradoOscuro = "#581C87";

            var documento = Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(1.8f, Unit.Centimetre);
                    pagina.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial).FontColor(colorTexto));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Background(colorPrincipal).Padding(14).Row(fila =>
                        {
                            fila.ConstantItem(85).Height(75).Background(Colors.White).Padding(8).AlignCenter().AlignMiddle().Element(e =>
                            {
                                if (File.Exists(rutaLogo))
                                    e.Image(rutaLogo).FitArea();
                                else
                                    e.Text("LOGO").FontSize(10).Bold().FontColor(colorPrincipal);
                            });

                            fila.RelativeItem().PaddingLeft(18).AlignMiddle().Column(titulo =>
                            {
                                titulo.Item().Text("COMPROBANTE DE VENTA")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);

                                titulo.Item().PaddingTop(4).Text("Librería Joelito")
                                    .FontSize(13)
                                    .SemiBold()
                                    .FontColor(colorMoradoClaro);

                                titulo.Item().PaddingTop(2).Text("Gracias por confiar en nosotros")
                                    .FontSize(9)
                                    .FontColor(colorMoradoMuyClaro);
                            });
                        });

                        col.Item().PaddingTop(14).Row(fila =>
                        {
                            fila.RelativeItem().Background(colorFondo).Border(1).BorderColor(colorBorde).Padding(10).Column(datos =>
                            {
                                datos.Item().Text("Datos del cliente").FontSize(10).Bold().FontColor(colorPrincipal);
                                datos.Item().PaddingTop(5).Text($"Fecha: {Convert.ToDateTime(cabecera["Fecha"]):dd/MM/yyyy}").Bold();
                                datos.Item().Text($"CI/NIT: {cabecera["Ci"]}").Bold();
                                datos.Item().Text($"Razón Social: {razonSocial}").Bold();
                            });

                            fila.ConstantItem(150).PaddingLeft(10).Background(colorMoradoMuyClaro).Border(1).BorderColor(colorBorde).Padding(10).Column(info =>
                            {
                                info.Item().Text("Comprobante").FontSize(10).Bold().FontColor(colorPrincipal);
                                info.Item().PaddingTop(5).Text("Estado: Emitido").FontSize(9);
                                info.Item().Text($"Hora: {DateTime.Now:HH:mm}").FontSize(9);
                            });
                        });
                    });

                    pagina.Content().PaddingVertical(18).Column(col =>
                    {
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(55);
                                c.RelativeColumn();
                                c.ConstantColumn(85);
                                c.ConstantColumn(90);
                            });

                            tabla.Header(h =>
                            {
                                h.Cell().Background(colorSecundario).Border(1).BorderColor(colorSecundario).Padding(6)
                                    .Text("Cant.").Bold().FontColor(Colors.White);

                                h.Cell().Background(colorSecundario).Border(1).BorderColor(colorSecundario).Padding(6)
                                    .Text("Descripción").Bold().FontColor(Colors.White);

                                h.Cell().Background(colorSecundario).Border(1).BorderColor(colorSecundario).Padding(6).AlignRight()
                                    .Text("P. Unit Bs.").Bold().FontColor(Colors.White);

                                h.Cell().Background(colorSecundario).Border(1).BorderColor(colorSecundario).Padding(6).AlignRight()
                                    .Text("Importe Bs.").Bold().FontColor(Colors.White);
                            });

                            foreach (DataRow fila in dt.Rows)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(colorBorde).Padding(6)
                                    .Text(fila["Cantidad"].ToString());

                                tabla.Cell().BorderBottom(1).BorderColor(colorBorde).Padding(6)
                                    .Text(fila["DescripcionProducto"].ToString());

                                tabla.Cell().BorderBottom(1).BorderColor(colorBorde).Padding(6).AlignRight()
                                    .Text($"{Convert.ToDecimal(fila["PrecioUnitario"]):N2} Bs.");

                                tabla.Cell().BorderBottom(1).BorderColor(colorBorde).Padding(6).AlignRight()
                                    .Text($"{Convert.ToDecimal(fila["Subtotal"]):N2} Bs.");
                            }
                        });

                        col.Item().PaddingTop(16).Row(fila =>
                        {
                            fila.RelativeItem().Background("#F8FAFC").Border(1).BorderColor(colorBorde).Padding(10).Column(son =>
                            {
                                son.Item().Text("Importe literal").FontSize(9).Bold().FontColor(colorPrincipal);
                                son.Item().PaddingTop(4).Text($"Son: {NumeroALetras(total)}").Bold();
                            });

                            fila.ConstantItem(190).PaddingLeft(12).Background(colorPrincipal).Padding(12).Column(totalBox =>
                            {
                                totalBox.Item().AlignRight().Text("TOTAL A PAGAR").FontSize(10).Bold().FontColor(colorMoradoClaro);
                                totalBox.Item().PaddingTop(3).AlignRight().Text($"{total:N2} Bs.").FontSize(18).Bold().FontColor(Colors.White);
                            });
                        });

                        col.Item().PaddingTop(18).Background(colorMoradoMuyClaro).Border(1).BorderColor(colorBorde).Padding(10).Text(
                            "Este comprobante respalda la venta realizada. Conserve este documento para cualquier consulta posterior."
                        ).FontSize(9).FontColor(colorMoradoOscuro);
                    });

                    pagina.Footer().BorderTop(1).BorderColor(colorBorde).PaddingTop(8).Row(fila =>
                    {
                        fila.RelativeItem().Text("Librería Joelito").FontSize(9).Bold().FontColor(colorPrincipal);

                        fila.RelativeItem().AlignCenter().Text("Gracias por su compra").FontSize(9).Italic().FontColor(colorMoradoOscuro);

                        fila.RelativeItem().AlignRight().Text($"{DateTime.Now:dd/MM/yyyy HH:mm} - {nombreEmpleado}")
                            .FontSize(9)
                            .Italic()
                            .FontColor(colorMoradoOscuro);
                    });
                });
            });

            return documento.GeneratePdf();
        }


        private string NumeroALetras(decimal numero)
        {
            long entero = (long)Math.Truncate(numero);
            int centavos = (int)Math.Round((numero - entero) * 100);

            string letras = entero == 0 ? "CERO" : ConvertirEnteroALetras(entero);

            letras = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(letras.ToLower());

            return $"{letras} {centavos:00}/100";
        }


        //TODO cambiar por una librería externa para convertir números a letras, o extraer a una clase de constantes aparte , o al menos optimizar este método que es muy largo y repetitivo
        private string ConvertirEnteroALetras(long numero)
        {
            if (numero == 0) return "";
            if (numero == 1) return "UN";
            if (numero == 2) return "DOS";
            if (numero == 3) return "TRES";
            if (numero == 4) return "CUATRO";
            if (numero == 5) return "CINCO";
            if (numero == 6) return "SEIS";
            if (numero == 7) return "SIETE";
            if (numero == 8) return "OCHO";
            if (numero == 9) return "NUEVE";
            if (numero == 10) return "DIEZ";
            if (numero == 11) return "ONCE";
            if (numero == 12) return "DOCE";
            if (numero == 13) return "TRECE";
            if (numero == 14) return "CATORCE";
            if (numero == 15) return "QUINCE";
            if (numero < 20) return "DIECI" + ConvertirEnteroALetras(numero - 10);
            if (numero == 20) return "VEINTE";
            if (numero < 30) return "VEINTI" + ConvertirEnteroALetras(numero - 20);
            if (numero == 30) return "TREINTA";
            if (numero == 40) return "CUARENTA";
            if (numero == 50) return "CINCUENTA";
            if (numero == 60) return "SESENTA";
            if (numero == 70) return "SETENTA";
            if (numero == 80) return "OCHENTA";
            if (numero == 90) return "NOVENTA";

            if (numero < 100) return ConvertirEnteroALetras((numero / 10) * 10) + " Y " + ConvertirEnteroALetras(numero % 10);

            if (numero == 100) return "CIEN";
            if (numero < 200) return "CIENTO " + ConvertirEnteroALetras(numero - 100);
            if (numero == 200) return "DOSCIENTOS";
            if (numero == 300) return "TRESCIENTOS";
            if (numero == 400) return "CUATROCIENTOS";
            if (numero == 500) return "QUINIENTOS";
            if (numero == 600) return "SEISCIENTOS";
            if (numero == 700) return "SETECIENTOS";
            if (numero == 800) return "OCHOCIENTOS";
            if (numero == 900) return "NOVECIENTOS";

            if (numero < 1000) return ConvertirEnteroALetras((numero / 100) * 100) + " " + ConvertirEnteroALetras(numero % 100);

            if (numero == 1000) return "MIL";
            if (numero < 2000) return "MIL " + ConvertirEnteroALetras(numero % 1000);

            if (numero < 1000000)
            {
                string miles = ConvertirEnteroALetras(numero / 1000) + " MIL";
                string resto = ConvertirEnteroALetras(numero % 1000);
                return resto == "" ? miles : miles + " " + resto;
            }

            if (numero == 1000000) return "UN MILLON";
            if (numero < 2000000) return "UN MILLON " + ConvertirEnteroALetras(numero % 1000000);

            if (numero < 1000000000000)
            {
                string millones = ConvertirEnteroALetras(numero / 1000000) + " MILLONES";
                string resto = ConvertirEnteroALetras(numero % 1000000);
                return resto == "" ? millones : millones + " " + resto;
            }

            return "";
        }
    }
}