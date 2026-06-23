using FrontendLibreria.DTOs;
using FrontendLibreria.DTOs.VentaDTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontendLibreria.Adaptadores.Producto
{
    public class AdaptadorProducto : IAdaptadorProducto
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AdaptadorProducto(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<ProductoDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/Producto");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProductoDto>>(_jsonOptions) ?? new();
        }

        public async Task<List<CategoriaDto>> GetCategoriasAsync()
        {
            return await _http.GetFromJsonAsync<List<CategoriaDto>>(
                "api/Producto/categorias",
                _jsonOptions
            ) ?? new();
        }

        public async Task<List<MarcaDto>> GetMarcasAsync()
        {
            return await _http.GetFromJsonAsync<List<MarcaDto>>(
                "api/Producto/marcas",
                _jsonOptions
            ) ?? new();
        }

        public async Task<List<PresentacionDto>> GetPresentacionesAsync()
        {
            return await _http.GetFromJsonAsync<List<PresentacionDto>>(
                "api/Producto/presentaciones",
                _jsonOptions
            ) ?? new();
        }

        public async Task<ResultadoProductoApi> UpdateAsync(ProductoDto producto)
        {
            var response = await _http.PutAsJsonAsync($"api/Producto/{producto.Id}", producto, _jsonOptions);

            if (response.IsSuccessStatusCode)
                return ResultadoProductoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorProductoApi>(_jsonOptions);

            return ResultadoProductoApi.Fail(
                error?.Errores ??
                new List<ErrorValidacionDto>
                {
                    new ErrorValidacionDto
                    {
                        Campo = "",
                        Mensaje = "Error desconocido"
                    }
                }
            );
        }

        public async Task<ResultadoApi> DeleteAsync(int id, int idUsuario)
        {
            var response = await _http.DeleteAsync($"api/Producto/{id}?idUsuario={idUsuario}");

            return response.IsSuccessStatusCode
                ? ResultadoApi.Ok()
                : ResultadoApi.Fail(new List<string> { "Error al eliminar" });
        }

        public async Task<ResultadoApi> AgregarPresentacionAsync(SolicitudAgregarPresentacion request)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/Producto/{request.IdProducto}/presentaciones",
                request,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
                return ResultadoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>(_jsonOptions);

            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }

        public async Task<ResultadoApi> CrearCategoriaAsync(string nombre, int idUsuario)
        {
            var response = await _http.PostAsJsonAsync(
                "api/Producto/categorias",
                new { Nombre = nombre, IdUsuario = idUsuario },
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
                return ResultadoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>(_jsonOptions);

            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error al crear categoría" });
        }

        public async Task<ResultadoProductoApi> CrearProductoAsync(
            ProductoDto producto,
            int idPresentacion,
            int factorConversion,
            decimal precioVenta)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/Producto/{idPresentacion}/{factorConversion}/{precioVenta}",
                producto,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
                return ResultadoProductoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorProductoApi>(_jsonOptions);

            return ResultadoProductoApi.Fail(
                error?.Errores ??
                new List<ErrorValidacionDto>
                {
                    new ErrorValidacionDto
                    {
                        Campo = "",
                        Mensaje = "Error desconocido"
                    }
                }
            );
        }

        public async Task<List<PresentacionProductoVentaDTO>> ObtenerPresentacionesPorFraseAsync(string frase)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(frase))
                    return new List<PresentacionProductoVentaDTO>();

                var response = await _http.GetAsync(
                    $"api/Producto/presentaciones/busqueda?frase={Uri.EscapeDataString(frase)}"
                );

                if (!response.IsSuccessStatusCode)
                    return new List<PresentacionProductoVentaDTO>();

                var resultado = await response.Content.ReadFromJsonAsync<List<PresentacionProductoVentaDTO>>(_jsonOptions);

                return resultado ?? new List<PresentacionProductoVentaDTO>();
            }
            catch
            {
                return new List<PresentacionProductoVentaDTO>();
            }
        }

        public async Task<PresentacionProductoVentaDTO?> ObtenerPresentacionProductoByIdsAsync(
            int idProducto,
            int idPresentacion)
        {
            try
            {
                var response = await _http.GetAsync(
                    $"api/Producto/productos/{idProducto}/presentaciones/{idPresentacion}"
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<PresentacionProductoVentaDTO>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}