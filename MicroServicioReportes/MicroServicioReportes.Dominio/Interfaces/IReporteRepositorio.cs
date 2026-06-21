using MicroServicioReportes.Dominio.Entidades.DTOs;

namespace MicroServicioReportes.Dominio.Interfaces;

public interface IReporteRepositorio
{
    Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(
        int idVenta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<VentaProductoReporteDto>> ObtenerVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ResumenRecaudacionReporteDto>> ObtenerResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default);
}
