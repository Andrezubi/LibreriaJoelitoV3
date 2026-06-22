using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;

namespace MicroServicioReportes.Infraestructura.Repositorios;

public class ReporteRepositorioEnMemoria : IReporteRepositorio
{
    private readonly List<VentaProductoReporteDto> _ventas = new()
    {
        new VentaProductoReporteDto
        {
            NumeroVenta = 1,
            FechaVenta = DateTime.Today.AddDays(-2),
            IdProducto = 1,
            IdPresentacion = 1,
            Producto = "Clean Code",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 2,
            PrecioUnitario = 95,
            IdCliente = 1,
            Cliente = "Cliente Demo",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 1,
            FechaVenta = DateTime.Today.AddDays(-2),
            IdProducto = 2,
            IdPresentacion = 1,
            Producto = "Arquitectura Limpia",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 1,
            PrecioUnitario = 120,
            IdCliente = 1,
            Cliente = "Cliente Demo",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 2,
            FechaVenta = DateTime.Today.AddDays(-1),
            IdProducto = 3,
            IdPresentacion = 1,
            Producto = "El Principito",
            Categoria = "Literatura",
            Presentacion = "Unidad",
            CantidadVendida = 3,
            PrecioUnitario = 45,
            IdCliente = 2,
            Cliente = "Maria Gomez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 3,
            FechaVenta = DateTime.Today,
            IdProducto = 1,
            IdPresentacion = 1,
            Producto = "Clean Code",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 1,
            PrecioUnitario = 95,
            IdCliente = 2,
            Cliente = "Maria Gomez",
            EstadoVenta = "Pendiente"
        }
    };

    public Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(
        int idVenta,
        CancellationToken cancellationToken = default)
    {
        var detalles = _ventas
            .Where(v => v.NumeroVenta == idVenta)
            .Select(v => new DetalleVentaReporteDto
            {
                Producto = v.Producto,
                Categoria = v.Categoria,
                Cantidad = v.CantidadVendida,
                PrecioUnitario = v.PrecioUnitario
            })
            .ToList();

        if (!detalles.Any())
        {
            return Task.FromResult<ComprobanteVentaDto?>(null);
        }

        var venta = _ventas.First(v => v.NumeroVenta == idVenta);
        var comprobante = new ComprobanteVentaDto
        {
            IdVenta = idVenta,
            FechaVenta = venta.FechaVenta,
            Estado = venta.EstadoVenta,
            Cliente = new ClienteReporteDto
            {
                Id = 1,
                RazonSocial = venta.Cliente,
                CiNit = "1234567"
            },
            Detalles = detalles,
            UsuarioGenerador = "Usuario Demo"
        };

        comprobante.TotalLiteral = $"Son {comprobante.Total:N2} Bolivianos";
        return Task.FromResult<ComprobanteVentaDto?>(comprobante);
    }

    public Task<IReadOnlyCollection<VentaProductoReporteDto>> ObtenerVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var resultado = AplicarFiltros(_ventas, request)
            .Where(v => v.EstadoVenta.Equals("Confirmada", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<VentaProductoReporteDto>>(resultado);
    }

    public Task<IReadOnlyCollection<ResumenRecaudacionReporteDto>> ObtenerResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ventas = AplicarFiltros(_ventas, request)
            .Where(v => v.EstadoVenta.Equals("Confirmada", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var totalGeneral = ventas.Sum(v => v.Importe);
        var resumen = ventas
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

        return Task.FromResult<IReadOnlyCollection<ResumenRecaudacionReporteDto>>(resumen);
    }

    private static IEnumerable<VentaProductoReporteDto> AplicarFiltros(
        IEnumerable<VentaProductoReporteDto> ventas,
        ReporteRequestDto request)
    {
        var consulta = ventas;

        if (request.FechaDesde.HasValue)
        {
            consulta = consulta.Where(v => v.FechaVenta.Date >= request.FechaDesde.Value.Date);
        }

        if (request.FechaHasta.HasValue)
        {
            consulta = consulta.Where(v => v.FechaVenta.Date <= request.FechaHasta.Value.Date);
        }

        if (request.IdProducto.HasValue)
        {
            consulta = consulta.Where(v => v.IdProducto == request.IdProducto.Value);
        }

        if (request.IdCliente.HasValue)
        {
            consulta = consulta.Where(v => v.IdCliente == request.IdCliente.Value);
        }

        return consulta;
    }
}
