using FrontendLibreria.DTOs;

namespace FrontendLibreria.Adaptadores
{
    public class AdaptadorCliente : IAdaptadorCliente
    {
        private readonly HttpClient _http;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public AdaptadorCliente(HttpClient http, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;

            var idUsuario = _httpContextAccessor.HttpContext?.User?.FindFirst("IdUsuario")?.Value;
            if (!string.IsNullOrEmpty(idUsuario))
            {
                if (_http.DefaultRequestHeaders.Contains("X-IdUsuario"))
                    _http.DefaultRequestHeaders.Remove("X-IdUsuario");
                _http.DefaultRequestHeaders.Add("X-IdUsuario", idUsuario);
            }

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<ResultadoApi> InsertarAsync(ClienteDto cliente)
        {
            var response = await _http.PostAsJsonAsync("api/Cliente", cliente);
            if (response.IsSuccessStatusCode) return ResultadoApi.Ok();
            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }
        public async Task<List<ClienteDto>> ObtenerTodoAsync()
        {
            var response = await _http.GetAsync("api/Cliente");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ClienteDto>>() ?? new();
        }

        public async Task<ResultadoApi> ActualizarAsync(ClienteDto cliente)
        {
            var response = await _http.PutAsJsonAsync($"api/Cliente/{cliente.Id}", cliente);
            if (response.IsSuccessStatusCode) return ResultadoApi.Ok();
            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }

        public async Task<ResultadoApi> EliminarAsync(int id, int idUsuario)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Cliente/{id}")
            {
                Content = JsonContent.Create(new { IdUsuario = idUsuario })
            };
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode
                ? ResultadoApi.Ok()
                : ResultadoApi.Fail(new List<string> { "Error al eliminar" });
        }

        public async Task<ClienteDto?> ObtenerPorCiAsync(string ci)
        {
            var response = await _http.GetAsync($"api/Cliente/buscar-ci/{Uri.EscapeDataString(ci)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ClienteDto>();
        }

        public async Task<List<ClienteDto>> ObtenerSimilaresPorCiAsync(string ci)
        {
            var response = await _http.GetAsync($"api/Cliente/similares-ci/{Uri.EscapeDataString(ci)}");

            if (!response.IsSuccessStatusCode)
                return new List<ClienteDto>();

            return await response.Content.ReadFromJsonAsync<List<ClienteDto>>() ?? new List<ClienteDto>();
        }
    }
}