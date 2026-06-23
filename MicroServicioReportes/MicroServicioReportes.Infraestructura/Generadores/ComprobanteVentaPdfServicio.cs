using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using Microsoft.Extensions.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MicroServicioReportes.Infraestructura.Generadores
{
    public class ComprobanteVentaPdfServicio : IComprobanteVentaPdfServicio
    {
        private readonly IHostEnvironment _env;
        private readonly IReporteBuilder _builder;
        private readonly IPlantillaReporteProveedor _plantillas;

        private const string MoradoPrincipal = "#7B35BE";
        private const string MoradoOscuro = "#4B1F73";
        private const string MoradoMedio = "#9D5FE0";
        private const string MoradoClaro = "#EFE3FA";
        private const string MoradoMuyClaro = "#F8F3FC";

        private const string Blanco = "#FFFFFF";
        private const string GrisTexto = "#374151";
        private const string GrisSuave = "#F3F4F6";
        private const string GrisBorde = "#E5E7EB";

        private const string Verde = "#16A34A";
        private const string Rojo = "#DC2626";

        public ComprobanteVentaPdfServicio(
            IHostEnvironment env,
            IReporteBuilder builder,
            IPlantillaReporteProveedor plantillas)
        {
            _env = env;
            _builder = builder;
            _plantillas = plantillas;
        }

        public byte[] GenerarComprobanteVenta(ComprobanteVenta comprobante)
        {
            if (comprobante == null || comprobante.Detalles == null || comprobante.Detalles.Count == 0)
                return Array.Empty<byte>();

            DocumentoReporte documento = ConstruirDocumentoReporte(comprobante);

            return RenderizarComprobanteVenta(documento, comprobante);
        }

        private DocumentoReporte ConstruirDocumentoReporte(ComprobanteVenta comprobante)
        {
            DocumentoReporte plantilla = _plantillas.ObtenerPlantilla(TipoReporte.ComprobanteVenta);

            var filasDetalle = comprobante.Detalles.Select(detalle =>
                new Dictionary<string, string>
                {
                    ["Cantidad"] = detalle.Cantidad.ToString(),
                    ["Producto"] = detalle.ProductoNombre,
                    ["PrecioUnitario"] = $"{detalle.PrecioUnitario:0.00} Bs.",
                    ["Subtotal"] = $"{detalle.Subtotal:0.00} Bs."
                });

            return _builder
                .UsarPlantilla(plantilla)
                .AgregarEncabezado(
                    $"Comprobante de Venta Nro. {comprobante.NumeroComprobante}",
                    comprobante.Estado.Equals("ANULADO", StringComparison.OrdinalIgnoreCase)
                        ? "Venta anulada"
                        : "Venta confirmada",
                    comprobante.UsuarioNombre)
                .AgregarDatosGenerales(new[]
                {
            CrearCampo("Fecha venta", comprobante.FechaVenta.ToString("dd/MM/yyyy HH:mm")),
            CrearCampo("Cliente", comprobante.ClienteNombre),
            CrearCampo("CI/NIT", comprobante.ClienteCiNit ?? "-"),
            CrearCampo("Estado", comprobante.Estado),
            CrearCampo("Vendedor", comprobante.UsuarioNombre),
            CrearCampo("Fecha emisión", comprobante.FechaGeneracion.ToString("dd/MM/yyyy HH:mm")),
            CrearCampo("Venta Nro.", comprobante.VentaId.ToString()),
            CrearCampo("Comprobante Nro.", comprobante.NumeroComprobante),
            CrearCampo("CorrelationId", comprobante.CorrelationId)
                })
                .AgregarTabla(
                    "Detalle de productos",
                    new[] { "Cantidad", "Producto", "PrecioUnitario", "Subtotal" },
                    filasDetalle)
                .AgregarResumen(new[]
                {
            CrearCampo("Importe literal", ConvertirMontoALiteral(comprobante.Total)),
            CrearCampo("Total a pagar", $"{comprobante.Total:0.00} Bs.")
                })
                .AgregarPie(comprobante.UsuarioNombre)
                .Construir();
        }

        private static CampoReporte CrearCampo(string etiqueta, string valor)
        {
            return new CampoReporte
            {
                Etiqueta = etiqueta,
                Valor = valor
            };
        }

        private byte[] RenderizarComprobanteVenta(
            DocumentoReporte documentoReporte,
            ComprobanteVenta comprobante)
        {
            string rutaLogo = Path.Combine(
                _env.ContentRootPath,
                "Recursos",
                "Imagenes",
                "logo-lib.png"
            );

            var tablaDetalle = documentoReporte.Tablas.FirstOrDefault();
            var datos = documentoReporte.DatosGenerales
                .ToDictionary(c => c.Etiqueta, c => c.Valor);

            string fechaVenta = ObtenerValor(datos, "Fecha venta");
            string cliente = ObtenerValor(datos, "Cliente");
            string ciNit = ObtenerValor(datos, "CI/NIT");
            string estado = ObtenerValor(datos, "Estado");
            string vendedor = ObtenerValor(datos, "Vendedor");
            string fechaEmision = ObtenerValor(datos, "Fecha emisión");
            string ventaNro = ObtenerValor(datos, "Venta Nro.");
            string comprobanteNro = ObtenerValor(datos, "Comprobante Nro.");
            string correlationId = ObtenerValor(datos, "CorrelationId");

            string importeLiteral = documentoReporte.Resumen
                .FirstOrDefault(c => c.Etiqueta == "Importe literal")?.Valor
                ?? ConvertirMontoALiteral(comprobante.Total);

            string totalTexto = documentoReporte.Resumen
                .FirstOrDefault(c => c.Etiqueta == "Total a pagar")?.Valor
                ?? $"{comprobante.Total:0.00} Bs.";

            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(1.5f, Unit.Centimetre);

                    pagina.DefaultTextStyle(x => x
                        .FontSize(10)
                        .FontFamily(Fonts.Arial)
                        .FontColor(GrisTexto));

                    pagina.Content()
                        .Border(1.5f)
                        .BorderColor(MoradoPrincipal)
                        .Padding(0)
                        .Column(col =>
                        {
                            // ENCABEZADO
                            col.Item()
                                .Background(MoradoPrincipal)
                                .PaddingVertical(14)
                                .PaddingHorizontal(22)
                                .Row(row =>
                                {
                                    row.ConstantItem(85).Height(65).Element(contenedor =>
                                    {
                                        if (File.Exists(rutaLogo))
                                        {
                                            contenedor
                                                .Background(Blanco)
                                                .Padding(5)
                                                .Image(rutaLogo)
                                                .FitArea();
                                        }
                                        else
                                        {
                                            contenedor
                                                .Background(Blanco)
                                                .Border(1)
                                                .BorderColor(Blanco)
                                                .AlignCenter()
                                                .AlignMiddle()
                                                .Text("LJ")
                                                .FontSize(18)
                                                .FontColor(MoradoPrincipal)
                                                .Bold();
                                        }
                                    });

                                    row.RelativeItem().PaddingLeft(18).Column(info =>
                                    {
                                        info.Item().Text(documentoReporte.Titulo.ToUpper())
                                            .FontSize(23)
                                            .Bold()
                                            .FontColor(Blanco);

                                        info.Item().PaddingTop(4).Text("Librería Joelito")
                                            .FontSize(14)
                                            .SemiBold()
                                            .FontColor(MoradoClaro);

                                        info.Item().PaddingTop(3).Text($"Venta Nro. {ventaNro}")
                                            .FontSize(10)
                                            .FontColor(Blanco);

                                        info.Item().Text($"Comprobante Nro. {comprobanteNro}")
                                            .FontSize(10)
                                            .FontColor(Blanco);
                                    });
                                });

                            // CONTENIDO
                            col.Item().Padding(22).Column(contenido =>
                            {
                                // DATOS GENERALES
                                contenido.Item().Row(row =>
                                {
                                    row.RelativeItem()
                                        .Background(MoradoMuyClaro)
                                        .BorderLeft(5)
                                        .BorderColor(MoradoPrincipal)
                                        .Padding(12)
                                        .Column(clienteCol =>
                                        {
                                            clienteCol.Item().Text("Datos del cliente")
                                                .FontSize(13)
                                                .Bold()
                                                .FontColor(MoradoOscuro);

                                            clienteCol.Item().PaddingTop(7).Text($"Fecha venta: {fechaVenta}");
                                            clienteCol.Item().Text($"Cliente: {cliente}");
                                            clienteCol.Item().Text($"CI/NIT: {ciNit}");
                                        });

                                    row.ConstantItem(20);

                                    row.RelativeItem()
                                        .Background(MoradoClaro)
                                        .BorderLeft(5)
                                        .BorderColor(MoradoMedio)
                                        .Padding(12)
                                        .Column(comp =>
                                        {
                                            comp.Item().Text("Comprobante")
                                                .FontSize(13)
                                                .Bold()
                                                .FontColor(MoradoOscuro);

                                            comp.Item().PaddingTop(7).Row(r =>
                                            {
                                                r.AutoItem().Text("Estado: ");
                                                r.AutoItem()
                                                    .Background(ObtenerColorEstado(estado))
                                                    .PaddingHorizontal(8)
                                                    .PaddingVertical(3)
                                                    .Text(estado)
                                                    .FontSize(9)
                                                    .FontColor(Blanco)
                                                    .Bold();
                                            });

                                            comp.Item().PaddingTop(5).Text($"Vendedor: {vendedor}");
                                            comp.Item().Text($"Fecha emisión: {fechaEmision}");
                                        });
                                });

                                // TÍTULO DETALLE
                                contenido.Item().PaddingTop(24)
                                    .Background(MoradoPrincipal)
                                    .PaddingVertical(8)
                                    .PaddingHorizontal(10)
                                    .Text(tablaDetalle?.Titulo ?? "Detalle de productos")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor(Blanco);

                                // TABLA DETALLE
                                contenido.Item().Table(tabla =>
                                {
                                    tabla.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(55);
                                        columns.RelativeColumn(4);
                                        columns.ConstantColumn(95);
                                        columns.ConstantColumn(95);
                                    });

                                    tabla.Header(header =>
                                    {
                                        CeldaCabecera(header, "Cant.");
                                        CeldaCabecera(header, "Descripción");
                                        CeldaCabecera(header, "P. Unit Bs.");
                                        CeldaCabecera(header, "Importe Bs.");
                                    });

                                    bool alternar = false;

                                    if (tablaDetalle != null)
                                    {
                                        foreach (var fila in tablaDetalle.Filas)
                                        {
                                            string fondo = alternar ? GrisSuave : Blanco;

                                            CeldaDetalle(tabla, ObtenerValor(fila, "Cantidad"), true, fondo);
                                            CeldaDetalle(tabla, ObtenerValor(fila, "Producto"), false, fondo);
                                            CeldaDetalle(tabla, ObtenerValor(fila, "PrecioUnitario"), true, fondo);
                                            CeldaDetalle(tabla, ObtenerValor(fila, "Subtotal"), true, fondo);

                                            alternar = !alternar;
                                        }
                                    }
                                });

                                // IMPORTE LITERAL Y TOTAL
                                contenido.Item().PaddingTop(22).Row(row =>
                                {
                                    row.RelativeItem()
                                        .Border(1)
                                        .BorderColor(GrisBorde)
                                        .Background(MoradoMuyClaro)
                                        .Padding(12)
                                        .Column(literal =>
                                        {
                                            literal.Item().Text("Importe literal")
                                                .FontSize(11)
                                                .Bold()
                                                .FontColor(MoradoOscuro);

                                            literal.Item().PaddingTop(5)
                                                .Text($"Son: {importeLiteral}")
                                                .FontSize(10);
                                        });

                                    row.ConstantItem(20);

                                    row.ConstantItem(180)
                                        .Background(MoradoPrincipal)
                                        .Padding(13)
                                        .Column(total =>
                                        {
                                            total.Item().AlignCenter().Text("TOTAL A PAGAR")
                                                .FontSize(11)
                                                .Bold()
                                                .FontColor(Blanco);

                                            total.Item().PaddingTop(6).AlignCenter().Text(totalTexto)
                                                .FontSize(20)
                                                .Bold()
                                                .FontColor(Blanco);
                                        });
                                });

                                // NOTA HISTÓRICA
                                contenido.Item().PaddingTop(20)
                                    .Background(MoradoMuyClaro)
                                    .BorderLeft(5)
                                    .BorderColor(MoradoPrincipal)
                                    .Padding(10)
                                    .Text("Este comprobante se genera con los datos históricos guardados en Ventas. Los cambios posteriores en Clientes o Productos no modifican este documento.")
                                    .FontSize(9)
                                    .FontColor(GrisTexto);
                            });
                        });

                    // PIE DE PÁGINA
                    pagina.Footer().PaddingHorizontal(35).PaddingBottom(18).Row(row =>
                    {
                        row.RelativeItem().Text("Librería Joelito")
                            .FontSize(9)
                            .SemiBold()
                            .FontColor(MoradoPrincipal);

                        row.RelativeItem().AlignCenter()
                            .Text("Gracias por su compra")
                            .FontSize(9)
                            .FontColor(MoradoPrincipal)
                            .Bold();

                        row.RelativeItem().AlignRight()
                            .Text(documentoReporte.PiePagina)
                            .FontSize(9)
                            .FontColor(GrisTexto);
                    });
                });
            }).GeneratePdf();
        }

        private static string ObtenerValor(
            IReadOnlyDictionary<string, string> datos,
            string clave)
        {
            return datos.TryGetValue(clave, out string? valor)
                ? valor
                : "-";
        }

        private static void CeldaCabecera(TableCellDescriptor tabla, string texto)
        {
            tabla.Cell()
                .Background(MoradoOscuro)
                .Border(1)
                .BorderColor(MoradoOscuro)
                .PaddingVertical(7)
                .PaddingHorizontal(5)
                .AlignCenter()
                .Text(texto)
                .FontColor(Blanco)
                .Bold();
        }

        private static void CeldaDetalle(TableDescriptor tabla, string texto, bool alinearDerecha, string fondo)
        {
            var celda = tabla.Cell()
                .Background(fondo)
                .BorderLeft(1)
                .BorderRight(1)
                .BorderBottom(1)
                .BorderColor(GrisBorde)
                .PaddingVertical(6)
                .PaddingHorizontal(5);

            if (alinearDerecha)
                celda.AlignRight().Text(texto);
            else
                celda.Text(texto);
        }

        private static string ObtenerColorEstado(string estado)
        {
            if (string.Equals(estado, "ANULADO", StringComparison.OrdinalIgnoreCase))
                return Rojo;

            return Verde;
        }

        private static string ConvertirMontoALiteral(decimal monto)
        {
            int parteEntera = (int)Math.Floor(monto);
            int centavos = (int)Math.Round((monto - parteEntera) * 100);

            if (centavos == 100)
            {
                parteEntera += 1;
                centavos = 0;
            }

            string literalEntero = NumeroALetras(parteEntera);

            return $"{literalEntero} {centavos:00}/100 Bolivianos";
        }

        private static string NumeroALetras(int numero)
        {
            if (numero == 0)
                return "Cero";

            string[] unidades =
            {
                "", "Uno", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho", "Nueve",
                "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciséis", "Diecisiete",
                "Dieciocho", "Diecinueve"
            };

            string[] decenas =
            {
                "", "", "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta",
                "Ochenta", "Noventa"
            };

            string[] centenas =
            {
                "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos",
                "Seiscientos", "Setecientos", "Ochocientos", "Novecientos"
            };

            if (numero == 100)
                return "Cien";

            if (numero < 20)
                return unidades[numero];

            if (numero < 100)
            {
                int d = numero / 10;
                int u = numero % 10;

                if (u == 0)
                    return decenas[d];

                if (d == 2)
                    return "Veinti" + unidades[u].ToLower();

                return decenas[d] + " Y " + unidades[u];
            }

            if (numero < 1000)
            {
                int c = numero / 100;
                int resto = numero % 100;

                if (resto == 0)
                    return centenas[c];

                return centenas[c] + " " + NumeroALetras(resto);
            }

            if (numero < 1000000)
            {
                int miles = numero / 1000;
                int resto = numero % 1000;

                string textoMiles = miles == 1
                    ? "Mil"
                    : NumeroALetras(miles) + " Mil";

                if (resto == 0)
                    return textoMiles;

                return textoMiles + " " + NumeroALetras(resto);
            }

            return numero.ToString();
        }
    }
}