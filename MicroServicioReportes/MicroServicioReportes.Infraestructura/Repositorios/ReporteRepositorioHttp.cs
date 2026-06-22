using System.Net.Http.Json;
using System.Text.Json;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;

namespace MicroServicioReportes.Infraestructura.Repositorios;

public class ReporteRepositorioHttp : IReporteRepositorio
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _ventasClient;
    private readonly HttpClient _productosClient;

    public ReporteRepositorioHttp(HttpClient ventasClient, HttpClient productosClient)
    {
        _ventasClient = ventasClient;
        _productosClient = productosClient;
    }

    public async Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(
        int idVenta,
        CancellationToken cancellationToken = default)
    {
        var ventaCompleta = await ObtenerVentaCompletaAsync(idVenta, cancellationToken);
        if (ventaCompleta?.Venta is null)
        {
            return null;
        }

        var productos = await ObtenerProductosAsync(cancellationToken);
        var detalles = ventaCompleta.Detalles.Select(detalle =>
        {
            productos.TryGetValue(detalle.IdProducto, out var producto);

            return new DetalleVentaReporteDto
            {
                Producto = ObtenerTexto(producto?.Nombre, detalle.Producto),
                Categoria = ObtenerTexto(producto?.NombreCategoria, "Sin categoria"),
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario
            };
        }).ToList();

        return new ComprobanteVentaDto
        {
            IdVenta = ventaCompleta.Venta.Id,
            FechaVenta = ventaCompleta.Venta.Fecha,
            Estado = ventaCompleta.Venta.EstadoVenta,
            Cliente = new ClienteReporteDto
            {
                Id = ventaCompleta.Venta.IdCliente,
                RazonSocial = ventaCompleta.Venta.RazonSocialCliente,
                CiNit = ventaCompleta.Venta.CiCompleto
            },
            Detalles = detalles,
            TotalLiteral = $"Son {ventaCompleta.Venta.Total:N2} Bolivianos",
            UsuarioGenerador = $"Usuario {ventaCompleta.Venta.IdUsuario}"
        };
    }

    public async Task<IReadOnlyCollection<VentaProductoReporteDto>> ObtenerVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ventas = await ObtenerResumenVentasAsync(cancellationToken);
        var productos = await ObtenerProductosAsync(cancellationToken);
        var filas = new List<VentaProductoReporteDto>();

        foreach (var venta in ventas.Where(v => EsVentaIncluida(v, request)))
        {
            var ventaCompleta = await ObtenerVentaCompletaAsync(venta.Id, cancellationToken);
            if (ventaCompleta?.Venta is null || !EsEstadoConfirmado(ventaCompleta.Venta.EstadoVenta))
            {
                continue;
            }

            foreach (var detalle in ventaCompleta.Detalles)
            {
                if (request.IdProducto.HasValue && detalle.IdProducto != request.IdProducto.Value)
                {
                    continue;
                }

                productos.TryGetValue(detalle.IdProducto, out var producto);

                filas.Add(new VentaProductoReporteDto
                {
                    NumeroVenta = ventaCompleta.Venta.Id,
                    FechaVenta = ventaCompleta.Venta.Fecha,
                    IdProducto = detalle.IdProducto,
                    IdPresentacion = detalle.IdPresentacion,
                    Producto = ObtenerTexto(producto?.Nombre, detalle.Producto),
                    Categoria = ObtenerTexto(producto?.NombreCategoria, "Sin categoria"),
                    Presentacion = ObtenerTexto(detalle.Presentacion, "Sin presentacion"),
                    CantidadVendida = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    IdCliente = ventaCompleta.Venta.IdCliente,
                    Cliente = ventaCompleta.Venta.RazonSocialCliente,
                    EstadoVenta = ventaCompleta.Venta.EstadoVenta
                });
            }
        }

        return filas.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<ResumenRecaudacionReporteDto>> ObtenerResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ventas = await ObtenerVentasPorProductoAsync(request, cancellationToken);
        var totalGeneral = ventas.Sum(v => v.Importe);

        return ventas
            .GroupBy(v => v.Categoria)
            .Select(grupo =>
            {
                var totalGrupo = grupo.Sum(v => v.Importe);
                return new ResumenRecaudacionReporteDto
                {
                    Grupo = grupo.Key,
                    CantidadVendida = grupo.Sum(v => v.CantidadVendida),
                    TotalRecaudado = totalGrupo,
                    Porcentaje = totalGeneral <= 0 ? 0 : totalGrupo * 100 / totalGeneral
                };
            })
            .ToList()
            .AsReadOnly();
    }

    private async Task<IReadOnlyDictionary<int, ProductoExternoDto>> ObtenerProductosAsync(
        CancellationToken cancellationToken)
    {
        var productos = await _productosClient.GetFromJsonAsync<List<ProductoExternoDto>>(
            "api/Producto",
            JsonOptions,
            cancellationToken) ?? new List<ProductoExternoDto>();

        return productos
            .GroupBy(p => p.Id)
            .ToDictionary(g => g.Key, g => g.First());
    }

    private async Task<List<VentaResumenExternoDto>> ObtenerResumenVentasAsync(
        CancellationToken cancellationToken)
    {
        return await _ventasClient.GetFromJsonAsync<List<VentaResumenExternoDto>>(
            "api/Venta",
            JsonOptions,
            cancellationToken) ?? new List<VentaResumenExternoDto>();
    }

    private async Task<VentaCompletaExternaDto?> ObtenerVentaCompletaAsync(
        int idVenta,
        CancellationToken cancellationToken)
    {
        return await _ventasClient.GetFromJsonAsync<VentaCompletaExternaDto>(
            $"api/Venta/{idVenta}/completa",
            JsonOptions,
            cancellationToken);
    }

    private static bool EsVentaIncluida(VentaResumenExternoDto venta, ReporteRequestDto request)
    {
        if (!EsEstadoConfirmado(venta.Estado))
        {
            return false;
        }

        if (request.FechaDesde.HasValue && venta.Fecha.Date < request.FechaDesde.Value.Date)
        {
            return false;
        }

        if (request.FechaHasta.HasValue && venta.Fecha.Date > request.FechaHasta.Value.Date)
        {
            return false;
        }

        if (request.IdCliente.HasValue && venta.IdCliente != request.IdCliente.Value)
        {
            return false;
        }

        return true;
    }

    private static bool EsEstadoConfirmado(string estado)
    {
        return estado.Equals("Confirmada", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("Confirmado", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("CONFIRMADA", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("CONFIRMADO", StringComparison.OrdinalIgnoreCase);
    }

    private static string ObtenerTexto(string? valor, string fallback)
    {
        return string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
    }

    private sealed class VentaResumenExternoDto
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class VentaCompletaExternaDto
    {
        public VentaCabeceraExternaDto Venta { get; set; } = new();
        public List<DetalleVentaExternoDto> Detalles { get; set; } = new();
    }

    private sealed class VentaCabeceraExternaDto
    {
        public int Id { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string RazonSocialCliente { get; set; } = string.Empty;
        public string CiCliente { get; set; } = string.Empty;
        public string? ComplementoCliente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        public string CiCompleto =>
            string.IsNullOrWhiteSpace(ComplementoCliente)
                ? CiCliente
                : $"{CiCliente}-{ComplementoCliente}";
    }

    private sealed class DetalleVentaExternoDto
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Presentacion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    private sealed class ProductoExternoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreCategoria { get; set; }
    }
}
