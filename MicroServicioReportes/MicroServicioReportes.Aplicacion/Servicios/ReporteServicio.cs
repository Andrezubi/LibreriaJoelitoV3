using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using SkiaSharp;
using ClosedXML.Excel;

namespace MicroServicioReportes.Aplicacion.Servicios;

public class ReporteServicio : IReporteServicio
{
    private readonly IReporteRepositorio _repositorio;
    private readonly IBitacoraReporteRepositorio _bitacoraRepositorio;
    private readonly IReporteBuilder _builder;
    private readonly IPlantillaReporteProveedor _plantillas;
    private readonly IGeneradorReporte _generador;

    public ReporteServicio(
        IReporteRepositorio repositorio,
        IBitacoraReporteRepositorio bitacoraRepositorio,
        IReporteBuilder builder,
        IPlantillaReporteProveedor plantillas,
        IGeneradorReporte generador)
    {
        _repositorio = repositorio;
        _bitacoraRepositorio = bitacoraRepositorio;
        _builder = builder;
        _plantillas = plantillas;
        _generador = generador;
    }

    public async Task<ReporteResponseDto> GenerarComprobanteVentaAsync(
        int idVenta,
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var venta = await _repositorio.ObtenerComprobanteVentaAsync(idVenta, cancellationToken);
        if (venta is null)
        {
            throw new InvalidOperationException($"No se encontro la venta {idVenta}.");
        }

        var usuario = ObtenerUsuario(request, venta.UsuarioGenerador);
        var plantilla = _plantillas.ObtenerPlantilla(TipoReporte.ComprobanteVenta);
        var detalle = venta.Detalles.Select(d => new Dictionary<string, string>
        {
            ["Cantidad"] = d.Cantidad.ToString(),
            ["Producto"] = d.Producto,
            ["Categoria"] = d.Categoria,
            ["Precio Unitario Bs"] = FormatoMoneda(d.PrecioUnitario),
            ["Importe Bs"] = FormatoMoneda(d.Importe)
        });

        var documento = _builder
            .UsarPlantilla(plantilla)
            .AgregarEncabezado(
                $"Comprobante de Venta Nro. {venta.IdVenta}",
                venta.Estado.Equals("Anulada", StringComparison.OrdinalIgnoreCase)
                    ? "VENTA ANULADA"
                    : "Venta confirmada",
                usuario)
            .AgregarDatosGenerales(new[]
            {
                Campo("Nro. venta", venta.IdVenta.ToString()),
                Campo("Fecha venta", venta.FechaVenta.ToString("dd/MM/yyyy HH:mm")),
                Campo("Cliente", venta.Cliente.RazonSocial),
                Campo("CI/NIT", venta.Cliente.CiNit),
                Campo("Estado", venta.Estado)
            })
            .AgregarTabla(
                "Detalle de la venta",
                new[] { "Cantidad", "Producto", "Categoria", "Precio Unitario Bs", "Importe Bs" },
                detalle)
            .AgregarResumen(new[]
            {
                Campo("Total Bs", FormatoMoneda(venta.Total)),
                Campo("Monto literal", string.IsNullOrWhiteSpace(venta.TotalLiteral)
                    ? "Pendiente de conversion literal"
                    : venta.TotalLiteral)
            })
            .AgregarPie(usuario)
            .Construir();

        return Renderizar(documento, $"ComprobanteVenta_{venta.IdVenta:000000}");
    }

    public async Task<ReporteResponseDto> GenerarListaVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var datosReporte = await ObtenerDatosVentasPorProductoAsync(request, cancellationToken);

        var filas = datosReporte.Ventas
            .Select(v => new Dictionary<string, string>
            {
                ["Nro."] = v.NumeroVenta.ToString(),
                ["Fecha"] = v.FechaVenta.ToString("dd/MM/yyyy"),
                ["Producto"] = v.Producto,
                ["Categoria"] = v.Categoria,
                ["Presentacion"] = v.Presentacion,
                ["Cantidad"] = v.CantidadVendida.ToString(),
                ["Precio Unitario Bs"] = FormatoMoneda(v.PrecioUnitario),
                ["Importe Bs"] = FormatoMoneda(v.Importe),
                ["Cliente"] = v.Cliente,
                ["Estado"] = v.EstadoVenta
            });

        var plantilla = _plantillas.ObtenerPlantilla(TipoReporte.ListaVentasPorProducto);

        var documento = _builder
            .UsarPlantilla(plantilla)
            .AgregarEncabezado(
                "Reporte de Ventas por Producto",
                "Lista ordenada combinando informacion de Ventas y Productos",
                datosReporte.Usuario)
            .AgregarDatosGenerales(CamposFiltro(datosReporte.Filtros))
            .AgregarDatosGenerales(datosReporte.MicroserviciosConsultados
                .Select((servicio, indice) => Campo($"Fuente {indice + 1}", servicio)))
            .AgregarTabla(
                "Ventas detalladas por producto",
                new[] { "Nro.", "Fecha", "Producto", "Categoria", "Presentacion", "Cantidad", "Precio Unitario Bs", "Importe Bs", "Cliente", "Estado" },
                filas)
            .AgregarResumen(new[]
            {
                Campo("Total unidades vendidas", datosReporte.TotalUnidadesVendidas.ToString()),
                Campo("Total recaudado Bs", FormatoMoneda(datosReporte.TotalRecaudado))
            })
            .AgregarPie(datosReporte.Usuario)
            .Construir();

        var reporte = Renderizar(documento, $"VentasPorProducto_{datosReporte.FechaGeneracion:yyyyMMddHHmm}");
        await RegistrarBitacoraVentasPorProductoAsync(datosReporte, cancellationToken);

        return reporte;
    }

    public async Task<ReporteVentasPorProductoDto> ObtenerDatosVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidarRangoFechas(request);
        var ordenPor = NormalizarOrden(request.OrdenPor);

        var ventas = (await _repositorio.ObtenerVentasPorProductoAsync(request, cancellationToken))
            .Where(v => v.EstadoVenta.Equals("Confirmada", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ventasOrdenadas = OrdenarVentas(ventas, ordenPor, request.Descendente)
            .ToList()
            .AsReadOnly();

        return new ReporteVentasPorProductoDto
        {
            FechaGeneracion = DateTime.Now,
            Usuario = ObtenerUsuario(request),
            Filtros = CopiarFiltros(request, ordenPor),
            MicroserviciosConsultados = new[]
            {
                "MicroServicioVentas: ventas, detalle y cliente",
                "MicroServicioProductos: nombre y categoria del producto"
            },
            Ventas = ventasOrdenadas,
            TotalUnidadesVendidas = ventasOrdenadas.Sum(v => v.CantidadVendida),
            TotalRecaudado = ventasOrdenadas.Sum(v => v.Importe)
        };
    }

    public async Task<ReporteResponseDto> GenerarResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidarRangoFechas(request);

        var agruparPor = NormalizarAgrupacion(request.AgruparPor);
        request.AgruparPor = agruparPor;

        var ventas = (await _repositorio.ObtenerVentasPorProductoAsync(request, cancellationToken))
            .Where(EsVentaConfirmada)
            .ToList();

        var resumen = AgruparResumenRecaudacion(ventas, agruparPor)
            .OrderByDescending(r => r.TotalRecaudado)
            .ToList();

        var usuario = ObtenerUsuario(request);
        var etiquetaAgrupacion = ObtenerEtiquetaAgrupacion(agruparPor);
        var tituloGrafico = $"Gráfico de torta por {etiquetaAgrupacion.ToLowerInvariant()}";

        var totalVentas = ventas.Select(v => v.NumeroVenta).Distinct().Count();
        var totalProductosVendidos = ventas.Sum(v => v.CantidadVendida);
        var totalRecaudado = ventas.Sum(v => v.Importe);

        static IContainer CeldaDatoGeneral(IContainer c) =>
            c.Border(1)
             .BorderColor(Colors.Grey.Lighten2)
             .Padding(6);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                // ENCABEZADO
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignCenter()
                           .Text("LIBRERÍA JOELITO")
                           .FontSize(14).Bold()
                           .FontColor(Color.FromHex("#1a237e"));

                        col.Item().AlignCenter()
                           .Text($"RECAUDACIÓN POR {etiquetaAgrupacion.ToUpperInvariant()}")
                           .FontSize(11).Bold();

                        col.Item().AlignCenter()
                           .Text($"Desde: {FormatearFechaFiltro(request.FechaDesde)}  al  {FormatearFechaFiltro(request.FechaHasta)}")
                           .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                });

                // CONTENIDO
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().PaddingBottom(12).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        tabla.Cell().Element(CeldaDatoGeneral).Text($"Agrupado por: {etiquetaAgrupacion}");
                        tabla.Cell().Element(CeldaDatoGeneral).Text($"Ventas incluidas: {totalVentas}");
                        tabla.Cell().Element(CeldaDatoGeneral).Text($"Productos vendidos: {totalProductosVendidos}");
                        tabla.Cell().Element(CeldaDatoGeneral).Text($"Total Bs: {totalRecaudado:N2}");
                    });

                    col.Item().PaddingBottom(12)
                       .Text("Fuentes consultadas: MicroServicioVentas (ventas, detalles, importes y cantidades) + MicroServicioProductos (producto y categoría). Se excluyen ventas anuladas o no confirmadas.")
                       .FontSize(9)
                       .FontColor(Colors.Grey.Darken2);

                    // Tabla sumariada
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        // Encabezados
                        static IContainer CeldaHeader(IContainer c) =>
                            c.Background(Color.FromHex("#1a237e"))
                             .Padding(6)
                             .AlignCenter();

                        tabla.Header(h =>
                        {
                            h.Cell().Element(CeldaHeader)
                             .Text(etiquetaAgrupacion).FontColor(Colors.White).Bold();
                            h.Cell().Element(CeldaHeader)
                             .Text("Ventas").FontColor(Colors.White).Bold();
                            h.Cell().Element(CeldaHeader)
                             .Text("Unidades").FontColor(Colors.White).Bold();
                            h.Cell().Element(CeldaHeader)
                             .Text("Total Recaudado Bs.").FontColor(Colors.White).Bold();
                            h.Cell().Element(CeldaHeader)
                             .Text("Participación").FontColor(Colors.White).Bold();
                        });

                        // Filas
                        bool par = false;
                        foreach (var item in resumen)
                        {
                            var bg = par ? Color.FromHex("#e8eaf6") : Colors.White;
                            par = !par;

                            tabla.Cell().Background(bg).Padding(6)
                                 .Text(item.Grupo);
                            tabla.Cell().Background(bg).Padding(6).AlignCenter()
                                 .Text(item.CantidadVentas.ToString());
                            tabla.Cell().Background(bg).Padding(6).AlignCenter()
                                 .Text(item.CantidadVendida.ToString());
                            tabla.Cell().Background(bg).Padding(6).AlignRight()
                                 .Text(item.TotalRecaudado.ToString("N2"));
                            tabla.Cell().Background(bg).Padding(6).AlignRight()
                                 .Text($"{item.Porcentaje:N2}%");
                        }

                        // Fila totales
                        tabla.Cell().Background(Color.FromHex("#c5cae9"))
                             .Padding(6).Text("TOTAL").Bold();
                        tabla.Cell().Background(Color.FromHex("#c5cae9"))
                             .Padding(6).AlignCenter()
                             .Text(totalVentas.ToString()).Bold();
                        tabla.Cell().Background(Color.FromHex("#c5cae9"))
                             .Padding(6).AlignCenter()
                             .Text(totalProductosVendidos.ToString()).Bold();
                        tabla.Cell().Background(Color.FromHex("#c5cae9"))
                             .Padding(6).AlignRight()
                             .Text(totalRecaudado.ToString("N2")).Bold();
                        tabla.Cell().Background(Color.FromHex("#c5cae9"))
                             .Padding(6).AlignRight()
                             .Text(totalRecaudado <= 0 ? "0.00%" : "100.00%").Bold();
                    });

                    // Gráfico de torta
                    byte[] graficoPng = GenerarGraficoTorta(resumen, tituloGrafico);
                    col.Item().PaddingTop(24).AlignCenter()
                       .Width(300).Height(300)
                       .Image(graficoPng).FitArea();
                });

                // PIE
                page.Footer().AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span($"Reporte generado por: {usuario} - ");
                        txt.Span($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - ");
                        txt.Span("Página ").FontSize(9);
                        txt.CurrentPageNumber().FontSize(9);
                        txt.Span(" de ").FontSize(9);
                        txt.TotalPages().FontSize(9);
                    });
            });
        });

        byte[] pdfBytes = document.GeneratePdf();

        await RegistrarBitacoraResumenRecaudacionAsync(
            request,
            resumen.Count,
            totalVentas,
            totalProductosVendidos,
            totalRecaudado,
            cancellationToken);

        return new ReporteResponseDto
        {
            Archivo = pdfBytes,
            ContentType = "application/pdf",
            NombreArchivo = $"ResumenRecaudacion_{DateTime.Now:yyyyMMddHHmm}.pdf"
        };
    }

    private ReporteResponseDto Renderizar(DocumentoReporte documento, string nombreBase)
    {
        return new ReporteResponseDto
        {
            Archivo = _generador.Generar(documento),
            ContentType = _generador.ContentType,
            NombreArchivo = $"{nombreBase}{_generador.Extension}"
        };
    }

    private static string ObtenerUsuario(ReporteRequestDto request, string? fallback = null)
    {
        if (!string.IsNullOrWhiteSpace(request.Usuario))
        {
            return request.Usuario.Trim();
        }

        return string.IsNullOrWhiteSpace(fallback) ? "Sistema" : fallback.Trim();
    }

    private static void ValidarRangoFechas(ReporteRequestDto request)
    {
        if (request.FechaDesde.HasValue &&
            request.FechaHasta.HasValue &&
            request.FechaDesde.Value.Date > request.FechaHasta.Value.Date)
        {
            throw new ArgumentException("La fecha desde no puede ser mayor que la fecha hasta.");
        }
    }

    private static IEnumerable<VentaProductoReporteDto> OrdenarVentas(
        IEnumerable<VentaProductoReporteDto> ventas,
        string ordenPor,
        bool descendente)
    {
        return ordenPor switch
        {
            "fecha" => descendente
                ? ventas.OrderByDescending(v => v.FechaVenta).ThenBy(v => v.NumeroVenta)
                : ventas.OrderBy(v => v.FechaVenta).ThenBy(v => v.NumeroVenta),
            "cantidad" => descendente
                ? ventas.OrderByDescending(v => v.CantidadVendida).ThenBy(v => v.Producto)
                : ventas.OrderBy(v => v.CantidadVendida).ThenBy(v => v.Producto),
            "importe" => descendente
                ? ventas.OrderByDescending(v => v.Importe).ThenBy(v => v.Producto)
                : ventas.OrderBy(v => v.Importe).ThenBy(v => v.Producto),
            _ => descendente
                ? ventas.OrderByDescending(v => v.Producto).ThenBy(v => v.FechaVenta)
                : ventas.OrderBy(v => v.Producto).ThenBy(v => v.FechaVenta)
        };
    }

    private static string NormalizarOrden(string? ordenPor)
    {
        var orden = (ordenPor ?? "producto").Trim().ToLowerInvariant();

        return orden switch
        {
            "" => "producto",
            "producto" => "producto",
            "fecha" => "fecha",
            "cantidad" => "cantidad",
            "cantidadvendida" => "cantidad",
            "importe" => "importe",
            "total" => "importe",
            _ => throw new ArgumentException(
                "El orden debe ser producto, fecha, cantidad o importe.")
        };
    }

    private static IEnumerable<CampoReporte> CamposFiltro(ReporteRequestDto request)
    {
        yield return Campo("Fecha desde", request.FechaDesde?.ToString("dd/MM/yyyy") ?? "Sin filtro");
        yield return Campo("Fecha hasta", request.FechaHasta?.ToString("dd/MM/yyyy") ?? "Sin filtro");
        yield return Campo("Producto", request.IdProducto?.ToString() ?? "Todos");
        yield return Campo("Cliente", request.IdCliente?.ToString() ?? "Todos");
        yield return Campo("Orden", request.OrdenPor);
        yield return Campo("Orden descendente", request.Descendente ? "Si" : "No");
        yield return Campo("Usuario generador", ObtenerUsuario(request));
    }

    private static ReporteRequestDto CopiarFiltros(ReporteRequestDto request, string ordenPor)
    {
        return new ReporteRequestDto
        {
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            IdProducto = request.IdProducto,
            IdCliente = request.IdCliente,
            IdUsuario = request.IdUsuario,
            Usuario = ObtenerUsuario(request),
            OrdenPor = ordenPor,
            Descendente = request.Descendente,
            AgruparPor = request.AgruparPor
        };
    }

    private static CampoReporte Campo(string etiqueta, string valor)
    {
        return new CampoReporte
        {
            Etiqueta = etiqueta,
            Valor = valor
        };
    }

    private static string FormatoMoneda(decimal valor)
    {
        return valor.ToString("N2");
    }

    private static IEnumerable<ResumenRecaudacionReporteDto> AgruparResumenRecaudacion(
        IReadOnlyCollection<VentaProductoReporteDto> ventas,
        string agruparPor)
    {
        var totalGeneral = ventas.Sum(v => v.Importe);

        return ventas
            .GroupBy(v => ObtenerClaveAgrupacion(v, agruparPor))
            .Select(grupo =>
            {
                var totalGrupo = grupo.Sum(v => v.Importe);

                return new ResumenRecaudacionReporteDto
                {
                    Grupo = grupo.Key,
                    CantidadVentas = grupo.Select(v => v.NumeroVenta).Distinct().Count(),
                    CantidadVendida = grupo.Sum(v => v.CantidadVendida),
                    TotalRecaudado = totalGrupo,
                    Porcentaje = totalGeneral <= 0 ? 0 : totalGrupo * 100 / totalGeneral
                };
            });
    }

    private static string ObtenerClaveAgrupacion(VentaProductoReporteDto venta, string agruparPor)
    {
        return agruparPor switch
        {
            "producto" => string.IsNullOrWhiteSpace(venta.Producto) ? "Sin producto" : venta.Producto.Trim(),
            "categoria" => string.IsNullOrWhiteSpace(venta.Categoria) ? "Sin categoría" : venta.Categoria.Trim(),
            _ => throw new ArgumentException("La agrupación debe ser producto o categoría.")
        };
    }

    private static string NormalizarAgrupacion(string? agruparPor)
    {
        var criterio = (agruparPor ?? "categoria").Trim().ToLowerInvariant();

        return criterio switch
        {
            "" => "categoria",
            "categoria" => "categoria",
            "producto" => "producto",
            _ => throw new ArgumentException("La agrupación debe ser producto o categoría.")
        };
    }

    private static string ObtenerEtiquetaAgrupacion(string agruparPor)
    {
        return agruparPor == "producto" ? "Producto" : "Categoría";
    }

    private static string FormatearFechaFiltro(DateTime? fecha)
    {
        return fecha?.ToString("dd/MM/yyyy") ?? "Sin filtro";
    }

    private static bool EsVentaConfirmada(VentaProductoReporteDto venta)
    {
        return venta.EstadoVenta.Equals("Confirmada", StringComparison.OrdinalIgnoreCase) ||
               venta.EstadoVenta.Equals("Confirmado", StringComparison.OrdinalIgnoreCase);
    }

    private byte[] GenerarGraficoTorta(List<ResumenRecaudacionReporteDto> datos, string titulo)
    {
        const int W = 600, H = 400;
        using var surface = SKSurface.Create(new SKImageInfo(W, H));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        if (!datos.Any())
        {
            using var snap = surface.Snapshot();
            return snap.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        // Colores del gráfico
        var colores = new[]
        {
            SKColor.Parse("#1a237e"), SKColor.Parse("#3949ab"),
            SKColor.Parse("#7986cb"), SKColor.Parse("#c5cae9"),
            SKColor.Parse("#ff7043"), SKColor.Parse("#ffa726"),
            SKColor.Parse("#66bb6a"), SKColor.Parse("#26c6da")
        };

        decimal total = datos.Sum(d => d.TotalRecaudado);
        if (total <= 0)
        {
            using var snap = surface.Snapshot();
            return snap.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        float startAngle = -90f;
        var rectTorta = new SKRect(60, 40, 360, 340);

        // Dibujar sectores
        for (int i = 0; i < datos.Count; i++)
        {
            float sweep = (float)(datos[i].TotalRecaudado / total * 360m);
            using var paint = new SKPaint
            {
                Color = colores[i % colores.Length],
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawArc(rectTorta, startAngle, sweep, true, paint);

            // Borde blanco entre sectores
            using var border = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2
            };
            canvas.DrawArc(rectTorta, startAngle, sweep, true, border);
            startAngle += sweep;
        }

        // Leyenda
        float leyY = 50;
        for (int i = 0; i < datos.Count; i++)
        {
            float porcentaje = (float)(datos[i].TotalRecaudado / total * 100m);

            using var rectPaint = new SKPaint
            {
                Color = colores[i % colores.Length],
                IsAntialias = true
            };
            canvas.DrawRect(new SKRect(375, leyY, 395, leyY + 14), rectPaint);

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };
            using var textFont = new SKFont(SKTypeface.Default, 11);
            canvas.DrawText(
                $"{datos[i].Grupo} ({porcentaje:F1}%)",
                400,
                leyY + 12,
                SKTextAlign.Left,
                textFont,
                textPaint);

            leyY += 24;
        }

        // Título del gráfico
        using var tituloPaint = new SKPaint
        {
            Color = SKColor.Parse("#1a237e"),
            IsAntialias = true
        };
        using var tituloTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        using var tituloFont = new SKFont(tituloTypeface, 14);
        canvas.DrawText(titulo, 150, 380, SKTextAlign.Left, tituloFont, tituloPaint);

        using var snapshot = surface.Snapshot();
        return snapshot.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    private async Task RegistrarBitacoraVentasPorProductoAsync(
        ReporteVentasPorProductoDto datosReporte,
        CancellationToken cancellationToken)
    {
        var filtros = datosReporte.Filtros;
        var descripcion =
            "Reporte de ventas por producto generado. " +
            $"Desde: {filtros.FechaDesde?.ToString("dd/MM/yyyy") ?? "Sin filtro"}, " +
            $"Hasta: {filtros.FechaHasta?.ToString("dd/MM/yyyy") ?? "Sin filtro"}, " +
            $"Producto: {filtros.IdProducto?.ToString() ?? "Todos"}, " +
            $"Cliente: {filtros.IdCliente?.ToString() ?? "Todos"}, " +
            $"Orden: {filtros.OrdenPor}, " +
            $"Descendente: {(filtros.Descendente ? "Si" : "No")}, " +
            $"Filas: {datosReporte.Ventas.Count}.";

        await _bitacoraRepositorio.RegistrarAsync(
            new BitacoraReporteDto
            {
                IdUsuario = filtros.IdUsuario ?? 0,
                Accion = "GENERAR",
                Tabla = "ReporteVentasPorProducto",
                Fecha = DateTime.Now,
                Descripcion = descripcion
            },
            cancellationToken);
    }

    private async Task RegistrarBitacoraResumenRecaudacionAsync(
        ReporteRequestDto filtros,
        int cantidadGrupos,
        int cantidadVentas,
        int cantidadProductosVendidos,
        decimal totalRecaudado,
        CancellationToken cancellationToken)
    {
        var descripcion =
            "Reporte sumariado de recaudación generado. " +
            $"Desde: {FormatearFechaFiltro(filtros.FechaDesde)}, " +
            $"Hasta: {FormatearFechaFiltro(filtros.FechaHasta)}, " +
            $"Agrupación: {filtros.AgruparPor}, " +
            $"Producto: {filtros.IdProducto?.ToString() ?? "Todos"}, " +
            $"Cliente: {filtros.IdCliente?.ToString() ?? "Todos"}, " +
            $"Grupos: {cantidadGrupos}, " +
            $"Ventas: {cantidadVentas}, " +
            $"Productos vendidos: {cantidadProductosVendidos}, " +
            $"Total recaudado Bs: {totalRecaudado:N2}.";

        await _bitacoraRepositorio.RegistrarAsync(
            new BitacoraReporteDto
            {
                IdUsuario = filtros.IdUsuario ?? 0,
                Accion = "GENERAR",
                Tabla = "ReporteResumenRecaudacion",
                Fecha = DateTime.Now,
                Descripcion = descripcion
            },
            cancellationToken);
    }
}
