using FrontendLibreria.DTOs.Reportes;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Web;

namespace FrontendLibreria.Adaptadores.Reporte
{
    public class ReporteAdapter : IReporteAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReporteAdapter> _logger;

        public ReporteAdapter(HttpClient httpClient, ILogger<ReporteAdapter> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<byte[]> GenerarVentasPorProductoAsync(ReporteRequestDto request)
        {
            try
            {
                var query = QueryStringHelper.ToQueryString(request);
                var response = await _httpClient.GetAsync($"api/Reportes/ventas-producto?{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ventas por producto.");
                return Array.Empty<byte>();
            }
        }

        public async Task<byte[]> GenerarResumenRecaudacionAsync(ReporteRequestDto request)
        {
            try
            {
                var query = QueryStringHelper.ToQueryString(request);
                var response = await _httpClient.GetAsync($"api/Reportes/resumen-recaudacion?{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar resumen de recaudación.");
                return Array.Empty<byte>();
            }
        }

        public async Task<byte[]> VerComprobanteVentaAsync(int idVenta)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Reportes/comprobante-venta/{idVenta}/ver");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ver comprobante de venta {IdVenta}.", idVenta);
                return Array.Empty<byte>();
            }
        }

        public async Task<List<BitacoraReporteDto>> ObtenerBitacoraAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Reportes/bitacora");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<BitacoraReporteDto>>() ?? new List<BitacoraReporteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la bitácora de reportes.");
                return new List<BitacoraReporteDto>();
            }
        }
    }

    // Helper simple para convertir el DTO a QueryString
    public static class QueryStringHelper
    {
        public static string ToQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null)!.ToString());

            return string.Join("&", properties);
        }
    }
}
