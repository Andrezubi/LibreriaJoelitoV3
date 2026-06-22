using MicroServicioReportes.Dominio.Entidades.DTOs;

namespace MicroServicioReportes.Aplicacion.Interfaces;

public interface IReporteServicio
{
    Task<ReporteResponseDto> GenerarComprobanteVentaAsync(
        int idVenta,
        ReporteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ReporteResponseDto> GenerarListaVentasPorProductoAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ReporteResponseDto> GenerarResumenRecaudacionAsync(
        ReporteRequestDto request,
        CancellationToken cancellationToken = default);
}
