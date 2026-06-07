using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Http;

namespace FrontendLibreria.Adaptadores.Producto
{
    // Adapters/ProductoAdapter.cs
    public class AdaptadorProducto : IAdaptadorProducto
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

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
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ProductoDto>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/Producto");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProductoDto>>() ?? new();
        }

        public async Task<List<CategoriaDto>> GetCategoriasAsync()
            => await _http.GetFromJsonAsync<List<CategoriaDto>>("api/Producto/categorias") ?? new();

        public async Task<List<MarcaDto>> GetMarcasAsync()
            => await _http.GetFromJsonAsync<List<MarcaDto>>("api/Producto/marcas") ?? new();

        public async Task<List<PresentacionDto>> GetPresentacionesAsync()
            => await _http.GetFromJsonAsync<List<PresentacionDto>>("api/Producto/presentaciones") ?? new();

        public async Task<ResultadoProductoApi> UpdateAsync(ProductoDto producto)
        {
            var response = await _http.PutAsJsonAsync($"api/Producto/{producto.Id}", producto);
            if (response.IsSuccessStatusCode) return ResultadoProductoApi.Ok();
            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorProductoApi>();
            return ResultadoProductoApi.Fail(error?.Errores ?? new List<ErrorValidacionDto> { new ErrorValidacionDto { Campo = "", Mensaje = "Error desconocido" } });
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
            var response = await _http.PostAsJsonAsync($"api/Producto/{request.IdProducto}/presentaciones", request);
            if (response.IsSuccessStatusCode) return ResultadoApi.Ok();
            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error desconocido" });
        }



        public async Task<ResultadoApi> CrearCategoriaAsync(string nombre, int idUsuario)
        {
            var response = await _http.PostAsJsonAsync("api/Producto/categorias", new { Nombre = nombre, IdUsuario = idUsuario });
            if (response.IsSuccessStatusCode) return ResultadoApi.Ok();

            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorApi>();
            return ResultadoApi.Fail(error?.Errores ?? new List<string> { "Error al crear categoría" });
        }
        public async Task<ResultadoProductoApi> CrearProductoAsync(ProductoDto producto,int idPresentacion, int factorConversion,decimal precioVenta)
        {
            var response = await _http.PostAsJsonAsync($"api/Producto/{idPresentacion}/{factorConversion}/{precioVenta}", producto);
            if (response.IsSuccessStatusCode) return ResultadoProductoApi.Ok();
            var error = await response.Content.ReadFromJsonAsync<RespuestaErrorProductoApi>();
            return ResultadoProductoApi.Fail(error?.Errores ?? new List<ErrorValidacionDto> {
                new ErrorValidacionDto
                {
                    Campo = "",
                    Mensaje = "Error desconocido"
                }
            });

        }                                                                                              


    }
}
