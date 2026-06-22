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
            FechaVenta = new DateTime(2026, 6, 5),
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
            FechaVenta = new DateTime(2026, 6, 10),
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
            FechaVenta = new DateTime(2026, 6, 15),
            IdProducto = 4,
            IdPresentacion = 1,
            Producto = "La Casa de los Espiritus",
            Categoria = "Literatura",
            Presentacion = "Unidad",
            CantidadVendida = 2,
            PrecioUnitario = 55,
            IdCliente = 3,
            Cliente = "Juan Perez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 4,
            FechaVenta = new DateTime(2026, 6, 20),
            IdProducto = 5,
            IdPresentacion = 1,
            Producto = "Design Patterns",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 1,
            PrecioUnitario = 85,
            IdCliente = 4,
            Cliente = "Carlos Lopez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 5,
            FechaVenta = new DateTime(2026, 6, 22),
            IdProducto = 6,
            IdPresentacion = 1,
            Producto = "Programacion en C#",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 2,
            PrecioUnitario = 75,
            IdCliente = 5,
            Cliente = "Ana Rodriguez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 6,
            FechaVenta = new DateTime(2026, 6, 25),
            IdProducto = 7,
            IdPresentacion = 1,
            Producto = "Cien Anos de Soledad",
            Categoria = "Literatura",
            Presentacion = "Unidad",
            CantidadVendida = 1,
            PrecioUnitario = 50,
            IdCliente = 6,
            Cliente = "Sofia Martinez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 7,
            FechaVenta = new DateTime(2026, 6, 28),
            IdProducto = 8,
            IdPresentacion = 1,
            Producto = "RESTful Web Services",
            Categoria = "Programacion",
            Presentacion = "Unidad",
            CantidadVendida = 3,
            PrecioUnitario = 90,
            IdCliente = 7,
            Cliente = "Roberto Garcia",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 8,
            FechaVenta = new DateTime(2026, 6, 30),
            IdProducto = 9,
            IdPresentacion = 1,
            Producto = "El Quijote",
            Categoria = "Literatura",
            Presentacion = "Unidad",
            CantidadVendida = 2,
            PrecioUnitario = 65,
            IdCliente = 8,
            Cliente = "Patricia Sanchez",
            EstadoVenta = "Confirmada"
        },
        new VentaProductoReporteDto
        {
            NumeroVenta = 9,
            FechaVenta = new DateTime(2026, 6, 30),
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
                Id = venta.IdCliente,
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
            .GroupBy(v => ObtenerClaveAgrupacion(v, request.AgruparPor))
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

    private static string ObtenerClaveAgrupacion(VentaProductoReporteDto venta, string? agruparPor)
    {
        var criterio = (agruparPor ?? "categoria").Trim().ToLowerInvariant();

        return criterio switch
        {
            "producto" => string.IsNullOrWhiteSpace(venta.Producto) ? "Sin producto" : venta.Producto.Trim(),
            "categoria" => string.IsNullOrWhiteSpace(venta.Categoria) ? "Sin categoría" : venta.Categoria.Trim(),
            _ => throw new ArgumentException("La agrupación debe ser producto o categoría.")
        };
    }
}
