using MicroServicioReportes.Dominio.Entidades.DTOs;

namespace MicroServicioReportes.Dominio.Interfaces;

public interface IBitacoraReporteRepositorio
{
    Task RegistrarAsync(BitacoraReporteDto entrada, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BitacoraReporteDto>> ObtenerTodoAsync(CancellationToken cancellationToken = default);
}
