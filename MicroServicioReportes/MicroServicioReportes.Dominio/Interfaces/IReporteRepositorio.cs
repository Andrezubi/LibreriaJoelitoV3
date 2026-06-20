using MicroServicioReportes.Dominio.Entidades.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Dominio.Interfaces
{
    public interface IReporteRepositorio
    {
        
        Task<List<UsuarioReporteDto>> ObtenerUsuariosAsync();
        Task<List<ReporteVentaCategoriaDto>> ObtenerVentasPorCategoriaAsync(
            DateTime fechaDesde, DateTime fechaHasta);
    }
}
