using FrontendLibreria.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontendLibreria.Adaptadores.Marca
{
    public class AdaptadorMarca : IAdaptadorMarca
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdaptadorMarca(HttpClient http, IHttpContextAccessor httpContextAccessor)
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
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<MarcaDto>> ObtenerTodoAsync()
        {
            var response = await _http.GetAsync("api/Marca");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MarcaDto>>() ?? new();
        }

        public async Task<ResultadoApi> InsertarAsync(MarcaDto marca)
        {
            var response = await _http.PostAsJsonAsync("api/Marca", marca);

            if (response.IsSuccessStatusCode)
                return ResultadoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }

        public async Task<ResultadoApi> ActualizarAsync(MarcaDto marca)
        {
            var response = await _http.PutAsJsonAsync($"api/Marca/{marca.Id}", marca);

            if (response.IsSuccessStatusCode)
                return ResultadoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }

        public async Task<ResultadoApi> EliminarAsync(int id, int idUsuario)
        {
            var response = await _http.DeleteAsync($"api/Marca/{id}?idUsuario={idUsuario}");

            return response.IsSuccessStatusCode
                ? ResultadoApi.Ok()
                : ResultadoApi.Fail(new List<string> { "Error al eliminar" });
        }
    }
}