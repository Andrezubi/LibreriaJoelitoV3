using FrontendLibreria.Adaptadores.Venta;
using FrontendLibreria.DTOs.VentaDTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontendLibreria.Adapters.Venta
{
    public class VentaAdapter : IVentaAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VentaAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            var idUsuario = _httpContextAccessor.HttpContext?.User?.FindFirst("IdUsuario")?.Value;

            if (!string.IsNullOrEmpty(idUsuario))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("X-IdUsuario"))
                    _httpClient.DefaultRequestHeaders.Remove("X-IdUsuario");

                _httpClient.DefaultRequestHeaders.Add("X-IdUsuario", idUsuario);
            }

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<VentaDTO>> CargarVentasAsync()
        {
            var ventas = await _httpClient.GetFromJsonAsync<List<VentaDTO>>("api/Venta");
            return ventas ?? new List<VentaDTO>();
        }

        public async Task<ApiResultDTO<int>?> RegistrarVentaAsync(RegistrarVentaRequestDTO request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Venta", request);
            return await response.Content.ReadFromJsonAsync<ApiResultDTO<int>>();
        }

        public async Task<ApiResultDTO<int>?> AnularVentaAsync(int idVenta, int idEmpleado)
        {
            var response = await _httpClient.PutAsync(
                $"api/Venta/{idVenta}/anular?idEmpleado={idEmpleado}",
                null
            );

            return await response.Content.ReadFromJsonAsync<ApiResultDTO<int>>();
        }

        

        public async Task<byte[]> GenerarComprobantePdfAsync(int idVenta)
        {
            return await _httpClient.GetByteArrayAsync(
                $"api/Venta/{idVenta}/comprobante"
            );
        }

        public async Task<VentaCompletaDTO?> ObtenerVentaCompletaAsync(int idVenta)
        {
            return await _httpClient.GetFromJsonAsync<VentaCompletaDTO>(
                $"api/Venta/{idVenta}/completa"
            );
        }

        public async Task<List<Reporte1DTO>> ObtenerReporteServiciosAsync()
        {
            try
            {
                var reporte = await _httpClient.GetFromJsonAsync<List<Reporte1DTO>>("api/Venta/reporte-servicios");
                return reporte ?? new List<Reporte1DTO>();
            }
            catch
            {
                return new List<Reporte1DTO>();
            }
        }
    }
}