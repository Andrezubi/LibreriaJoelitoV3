using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;

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

        var resumen = await _repositorio.ObtenerResumenRecaudacionAsync(request, cancellationToken);
        var usuario = ObtenerUsuario(request);
        var filas = resumen
            .OrderByDescending(r => r.TotalRecaudado)
            .Select(r => new Dictionary<string, string>
            {
                ["Producto/Categoria"] = r.Grupo,
                ["Cantidad Vendida"] = r.CantidadVendida.ToString(),
                ["Total Recaudado Bs"] = FormatoMoneda(r.TotalRecaudado),
                ["Participacion"] = $"{r.Porcentaje:N2}%"
            });

        var plantilla = _plantillas.ObtenerPlantilla(TipoReporte.ResumenRecaudacion);

        var documento = _builder
            .UsarPlantilla(plantilla)
            .AgregarEncabezado(
                "Reporte Sumariado de Recaudacion",
                "Resumen con grafico estadistico para analizar el rendimiento de ventas",
                usuario)
            .AgregarDatosGenerales(CamposFiltro(request))
            .AgregarTabla(
                "Resumen de recaudacion",
                new[] { "Producto/Categoria", "Cantidad Vendida", "Total Recaudado Bs", "Participacion" },
                filas)
            .AgregarResumen(new[]
            {
                Campo("Total unidades vendidas", resumen.Sum(r => r.CantidadVendida).ToString()),
                Campo("Total recaudado Bs", FormatoMoneda(resumen.Sum(r => r.TotalRecaudado)))
            })
            .AgregarGrafico(
                "Distribucion de recaudacion",
                "Barras",
                resumen.Select(r => Campo(r.Grupo, $"{r.Porcentaje:N2}%")))
            .AgregarPie(usuario)
            .Construir();

        return Renderizar(documento, $"ResumenRecaudacion_{DateTime.Now:yyyyMMddHHmm}");
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
            Descendente = request.Descendente
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
}
