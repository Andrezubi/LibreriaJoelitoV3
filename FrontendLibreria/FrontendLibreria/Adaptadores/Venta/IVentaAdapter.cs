using FrontendLibreria.DTOs.VentaDTOs;

namespace FrontendLibreria.Adapters.Venta
{
    public interface IVentaAdapter
    {
        Task<List<VentaDTO>> CargarVentasAsync();

        Task<ApiResultDTO<int>?> RegistrarVentaAsync(RegistrarVentaRequestDTO request);

        Task<ApiResultDTO<int>?> AnularVentaAsync(int idVenta, int idEmpleado);

        Task<List<PresentacionProductoVentaDTO>> ObtenerPresentacionesPorFraseAsync(string frase);

        Task<PresentacionProductoVentaDTO?> ObtenerPresentacionProductoByIdsAsync(int idProducto, int idPresentacion);

        Task<byte[]> GenerarComprobantePdfAsync(int idVenta);

        Task<VentaCompletaDTO?> ObtenerVentaCompletaAsync(int idVenta);

        Task<List<Reporte1DTO>> ObtenerReporteServiciosAsync();
    }
}