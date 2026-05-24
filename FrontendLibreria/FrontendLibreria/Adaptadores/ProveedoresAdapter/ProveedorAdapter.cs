using System.Net.Http.Json;
using FrontendLibreria.DTOs.Proveedores;

namespace FrontendLibreria.Adaptadores.ProveedoresAdapter
{
    public class ProveedorAdapter : IProveedorAdapter
    {
        private readonly HttpClient _httpClient;

        public ProveedorAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProveedorDto>> ObtenerTodosAsync()
        {
            var proveedores = await _httpClient.GetFromJsonAsync<List<ProveedorDto>>("api/Proveedor");

            return proveedores ?? new List<ProveedorDto>();
        }

        public async Task<ProveedorDto?> ObtenerPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var response = await _httpClient.GetAsync($"api/Proveedor/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProveedorDto>();
        }

        public async Task<bool> RegistrarAsync(RegistrarProveedorDto proveedor)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Proveedor", proveedor);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarAsync(string id, RegistrarProveedorDto proveedor)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            proveedor.Id = id;

            var response = await _httpClient.PutAsJsonAsync($"api/Proveedor/{id}", proveedor);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var response = await _httpClient.DeleteAsync($"api/Proveedor/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}