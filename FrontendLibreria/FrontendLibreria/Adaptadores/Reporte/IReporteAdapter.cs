using FrontendLibreria.DTOs.Reportes;
using System.Threading.Tasks;

namespace FrontendLibreria.Adaptadores.Reporte
{
    public interface IReporteAdapter
    {
        Task<byte[]> GenerarVentasPorProductoAsync(ReporteRequestDto request);
        Task<byte[]> GenerarResumenRecaudacionAsync(ReporteRequestDto request);
        Task<byte[]> VerComprobanteVentaAsync(int idVenta);
        Task<List<BitacoraReporteDto>> ObtenerBitacoraAsync();
    }
}
