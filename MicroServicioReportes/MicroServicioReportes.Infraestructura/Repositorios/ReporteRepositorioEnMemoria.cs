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
            FechaVenta = new DateTime(2026, 6, 5),
            Producto = "Clean Code",
            Categoria = "Programacion",
            CantidadVendida = 2,
            PrecioUnitario = 95,
            Cliente = "Cliente Demo",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 1,
            FechaVenta = new DateTime(2026, 6, 5),
            Producto = "Arquitectura Limpia",
            Categoria = "Programacion",
            CantidadVendida = 1,
            PrecioUnitario = 120,
            Cliente = "Cliente Demo",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 2,
            FechaVenta = new DateTime(2026, 6, 10),
            Producto = "El Principito",
            Categoria = "Literatura",
            CantidadVendida = 3,
            PrecioUnitario = 45,
            Cliente = "Maria Gomez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 3,
            FechaVenta = new DateTime(2026, 6, 15),
            Producto = "La Casa de los Espiritus",
            Categoria = "Literatura",
            CantidadVendida = 2,
            PrecioUnitario = 55,
            Cliente = "Juan Perez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 4,
            FechaVenta = new DateTime(2026, 6, 20),
            Producto = "Design Patterns",
            Categoria = "Programacion",
            CantidadVendida = 1,
            PrecioUnitario = 85,
            Cliente = "Carlos Lopez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 5,
            FechaVenta = new DateTime(2026, 6, 22),
            Producto = "Programacion en C#",
            Categoria = "Programacion",
            CantidadVendida = 2,
            PrecioUnitario = 75,
            Cliente = "Ana Rodriguez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 6,
            FechaVenta = new DateTime(2026, 6, 25),
            Producto = "Cien Años de Soledad",
            Categoria = "Literatura",
            CantidadVendida = 1,
            PrecioUnitario = 50,
            Cliente = "Sofia Martinez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 7,
            FechaVenta = new DateTime(2026, 6, 28),
            Producto = "RESTful Web Services",
            Categoria = "Programacion",
            CantidadVendida = 3,
            PrecioUnitario = 90,
            Cliente = "Roberto Garcia",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 8,
            FechaVenta = new DateTime(2026, 6, 30),
            Producto = "El Quijote",
            Categoria = "Literatura",
            CantidadVendida = 2,
            PrecioUnitario = 65,
            Cliente = "Patricia Sanchez",
            EstadoVenta = "Confirmada"
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
            .Where(v => !v.EstadoVenta.Equals("Anulada", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<VentaProductoReporteDto>>(resultado);
    }

    public Task<IReadOnlyCollection<ResumenRecaudacionReporteDto>> ObtenerResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ventas = AplicarFiltros(_ventas, request)
            .Where(v => !v.EstadoVenta.Equals("Anulada", StringComparison.OrdinalIgnoreCase))
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

        return consulta;
    }
}
