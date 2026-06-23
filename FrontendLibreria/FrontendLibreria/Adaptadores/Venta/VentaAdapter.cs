using FrontendLibreria.DTOs.VentaDTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontendLibreria.Adaptadores.Venta
{
    public class VentaAdapter : IVentaAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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
            try
            {
                var response = await _httpClient.GetAsync("api/Venta");

                if (!response.IsSuccessStatusCode)
                    return new List<VentaDTO>();

                var ventas = await response.Content.ReadFromJsonAsync<List<VentaDTO>>(_jsonOptions);

                return ventas ?? new List<VentaDTO>();
            }
            catch
            {
                return new List<VentaDTO>();
            }
        }

        public async Task<ApiResultDTO<ResultadoInicioVentaSagaDTO>?> RegistrarVentaAsync(RegistrarVentaRequestDTO request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Venta", request, _jsonOptions);

            return await response.Content.ReadFromJsonAsync<ApiResultDTO<ResultadoInicioVentaSagaDTO>>(_jsonOptions);
        }

        public async Task<ApiResultDTO<int>?> AnularVentaAsync(int idVenta, int idUsuario)
        {
            var response = await _httpClient.PutAsync(
                $"api/Venta/{idVenta}/anular?idUsuario={idUsuario}",
                null
            );

            return await response.Content.ReadFromJsonAsync<ApiResultDTO<int>>(_jsonOptions);
        }

        public async Task<byte[]> GenerarComprobantePdfAsync(int idVenta)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Venta/{idVenta}/comprobante");

                if (!response.IsSuccessStatusCode)
                    return Array.Empty<byte>();

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public async Task<VentaCompletaDTO?> ObtenerVentaCompletaAsync(int idVenta)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Venta/{idVenta}/completa");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<VentaCompletaDTO>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Reporte1DTO>> ObtenerReporteServiciosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Venta/reporte-servicios");

                if (!response.IsSuccessStatusCode)
                    return new List<Reporte1DTO>();

                var reporte = await response.Content.ReadFromJsonAsync<List<Reporte1DTO>>(_jsonOptions);

                return reporte ?? new List<Reporte1DTO>();
            }
            catch
            {
                return new List<Reporte1DTO>();
            }
        }
    }
}