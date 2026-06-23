using FrontendLibreria.DTOs.Reportes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FrontendLibreria.Adaptadores.Reporte
{
    public class ReporteAdapter : IReporteAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReporteAdapter> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReporteAdapter(
            HttpClient httpClient,
            ILogger<ReporteAdapter> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<byte[]> GenerarVentasPorProductoAsync(ReporteRequestDto request)
        {
            return await ObtenerPdfAsync(
                "api/Reportes/ventas-producto",
                request,
                "ventas por producto");
        }

        public async Task<byte[]> GenerarResumenRecaudacionAsync(ReporteRequestDto request)
        {
            return await ObtenerPdfAsync(
                "api/Reportes/resumen-recaudacion",
                request,
                "resumen de recaudacion");
        }

        public async Task<byte[]> VerComprobanteVentaAsync(int idVenta)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Reportes/comprobante-venta/{idVenta}/ver");

                AgregarCabecerasUsuario(request);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al ver comprobante de venta {IdVenta}. Codigo: {StatusCode}. Respuesta: {Respuesta}",
                        idVenta,
                        (int)response.StatusCode,
                        error);

                    return Array.Empty<byte>();
                }

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
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/Reportes/bitacora");
                AgregarCabecerasUsuario(request);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener la bitacora de reportes. Codigo: {StatusCode}. Respuesta: {Respuesta}",
                        (int)response.StatusCode,
                        error);

                    return new List<BitacoraReporteDto>();
                }

                return await response.Content.ReadFromJsonAsync<List<BitacoraReporteDto>>()
                    ?? new List<BitacoraReporteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la bitacora de reportes.");
                return new List<BitacoraReporteDto>();
            }
        }

        private async Task<byte[]> ObtenerPdfAsync(
            string endpoint,
            ReporteRequestDto request,
            string nombreReporte)
        {
            try
            {
                var query = QueryStringHelper.ToQueryString(request);
                var url = string.IsNullOrWhiteSpace(query)
                    ? endpoint
                    : $"{endpoint}?{query}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
                AgregarCabecerasUsuario(httpRequest);

                var response = await _httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al generar reporte de {NombreReporte}. Codigo: {StatusCode}. Respuesta: {Respuesta}",
                        nombreReporte,
                        (int)response.StatusCode,
                        error);

                    return Array.Empty<byte>();
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de {NombreReporte}.", nombreReporte);
                return Array.Empty<byte>();
            }
        }

        private void AgregarCabecerasUsuario(HttpRequestMessage request)
        {
            var usuario = _httpContextAccessor.HttpContext?.User;
            if (usuario is null)
            {
                return;
            }

            var idUsuario = usuario.FindFirst("IdUsuario")?.Value
                ?? usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(idUsuario))
            {
                request.Headers.TryAddWithoutValidation("X-IdUsuario", idUsuario);
            }

            var token = usuario.FindFirst("Token")?.Value;
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    // Convierte el DTO a query string usando formatos estables para la API.
    public static class QueryStringHelper
    {
        public static string ToQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             let value = ConvertirValor(p.GetValue(obj, null))
                             where !string.IsNullOrWhiteSpace(value)
                             select Uri.EscapeDataString(p.Name) + "=" + Uri.EscapeDataString(value);

            return string.Join("&", properties);
        }

        private static string ConvertirValor(object? value)
        {
            return value switch
            {
                null => string.Empty,
                string texto => texto.Trim(),
                DateTime fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                bool bandera => bandera.ToString().ToLowerInvariant(),
                IFormattable formateable => formateable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()?.Trim() ?? string.Empty
            };
        }
    }
}
