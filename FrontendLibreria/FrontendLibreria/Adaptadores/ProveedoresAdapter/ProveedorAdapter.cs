using FrontendLibreria.DTOs.Proveedores;
using System.Net.Http.Json;
using System.Text.Json;

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
            var resultado = await RegistrarConResultadoAsync(proveedor);

            return resultado.Exitoso;
        }

        public async Task<ProveedorOperacionResultadoDTO> RegistrarConResultadoAsync(RegistrarProveedorDto proveedor)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Proveedor", proveedor);

            if (response.IsSuccessStatusCode)
            {
                return new ProveedorOperacionResultadoDTO
                {
                    Exitoso = true,
                    Mensaje = "Proveedor registrado exitosamente."
                };
            }

            var resultado = new ProveedorOperacionResultadoDTO
            {
                Exitoso = false,
                Mensaje = "No se pudo registrar el proveedor."
            };

            var errores = await ObtenerErroresDesdeRespuestaAsync(response);

            if (!errores.Any())
            {
                resultado.ErroresGenerales.Add("No se pudo registrar el proveedor. Verifique los datos ingresados.");
                return resultado;
            }

            foreach (var error in errores)
            {
                var campo = ObtenerCampoDesdeMensaje(error);

                if (string.IsNullOrWhiteSpace(campo))
                {
                    resultado.ErroresGenerales.Add(error);
                    continue;
                }

                if (!resultado.ErroresPorCampo.ContainsKey(campo))
                {
                    resultado.ErroresPorCampo[campo] = new List<string>();
                }

                resultado.ErroresPorCampo[campo].Add(error);
            }

            return resultado;
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

        private static async Task<List<string>> ObtenerErroresDesdeRespuestaAsync(HttpResponseMessage response)
        {
            var contenido = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(contenido))
            {
                return new List<string>();
            }

            try
            {
                using var documento = JsonDocument.Parse(contenido);
                var raiz = documento.RootElement;

                if (!raiz.TryGetProperty("error", out var errorElement))
                {
                    return new List<string>
                    {
                        contenido
                    };
                }

                if (errorElement.ValueKind == JsonValueKind.Array)
                {
                    return errorElement
                        .EnumerateArray()
                        .Select(e => e.GetString())
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Select(e => e!)
                        .ToList();
                }

                if (errorElement.ValueKind == JsonValueKind.String)
                {
                    var error = errorElement.GetString();

                    return string.IsNullOrWhiteSpace(error)
                        ? new List<string>()
                        : new List<string> { error };
                }

                return new List<string>
                {
                    errorElement.ToString()
                };
            }
            catch
            {
                return new List<string>
                {
                    contenido
                };
            }
        }

        private static string? ObtenerCampoDesdeMensaje(string mensaje)
        {
            var texto = mensaje.ToLowerInvariant();

            if (texto.Contains("nombre"))
            {
                return nameof(RegistrarProveedorDto.Nombre);
            }

            if (texto.Contains("nit"))
            {
                return nameof(RegistrarProveedorDto.Nit);
            }

            if (texto.Contains("teléfono") ||
                texto.Contains("telefono") ||
                texto.Contains("número de teléfono") ||
                texto.Contains("numero de telefono"))
            {
                return nameof(RegistrarProveedorDto.TelefonoContacto);
            }

            if (texto.Contains("dirección") || texto.Contains("direccion"))
            {
                return nameof(RegistrarProveedorDto.Direccion);
            }

            if (texto.Contains("descripción") || texto.Contains("descripcion"))
            {
                return nameof(RegistrarProveedorDto.Descripcion);
            }

            if (texto.Contains("usuario"))
            {
                return nameof(RegistrarProveedorDto.IdUsuario);
            }

            return null;
        }
    }
}