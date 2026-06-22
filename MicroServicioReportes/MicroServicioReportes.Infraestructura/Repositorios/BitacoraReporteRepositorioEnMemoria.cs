using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;

namespace MicroServicioReportes.Infraestructura.Repositorios;

public class BitacoraReporteRepositorioEnMemoria : IBitacoraReporteRepositorio
{
    private readonly List<BitacoraReporteDto> _entradas = new();
    private readonly object _bloqueo = new();
    private int _secuencia;

    public Task RegistrarAsync(
        BitacoraReporteDto entrada,
        CancellationToken cancellationToken = default)
    {
        lock (_bloqueo)
        {
            entrada.Id = ++_secuencia;
            entrada.Fecha = entrada.Fecha == default ? DateTime.Now : entrada.Fecha;
            _entradas.Add(entrada);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<BitacoraReporteDto>> ObtenerTodoAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_bloqueo)
        {
            return Task.FromResult<IReadOnlyCollection<BitacoraReporteDto>>(
                _entradas
                    .OrderByDescending(e => e.Fecha)
                    .Select(e => new BitacoraReporteDto
                    {
                        Id = e.Id,
                        IdUsuario = e.IdUsuario,
                        Accion = e.Accion,
                        Tabla = e.Tabla,
                        Fecha = e.Fecha,
                        Descripcion = e.Descripcion
                    })
                    .ToList()
                    .AsReadOnly());
        }
    }
}
