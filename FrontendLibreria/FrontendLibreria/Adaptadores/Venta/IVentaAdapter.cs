using FrontendLibreria.DTOs.VentaDTOs;

namespace FrontendLibreria.Adaptadores.Venta
{
    public interface IVentaAdapter
    {
        Task<List<VentaDTO>> CargarVentasAsync();

        Task<ApiResultDTO<ResultadoInicioVentaSagaDTO>?> RegistrarVentaAsync(RegistrarVentaRequestDTO request);

        Task<ApiResultDTO<int>?> AnularVentaAsync(int idVenta, int idUsuario);

        Task<byte[]> GenerarComprobantePdfAsync(int idVenta);

        Task<VentaCompletaDTO?> ObtenerVentaCompletaAsync(int idVenta);

        Task<List<Reporte1DTO>> ObtenerReporteServiciosAsync();
    }
}