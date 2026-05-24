using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontendLibreria.Adaptadores
{
    public class UsuarioServicioAdapter : IUsuarioServicioAdapter
    {
        private readonly HttpClient _httpClient;
        ILogger<UsuarioServicioAdapter> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioServicioAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<UsuarioServicioAdapter> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public ILogger<UsuarioServicioAdapter> GetLogger()
        {
            return _logger;
        }

        /// <summary>
        /// Obtiene el token del usuario actual desde los claims de la cookie
        /// </summary>
        private string? ObtenerTokenDelUsuarioActual()
        {
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("✅ Token obtenido del usuario actual");
                return token;
            }

            _logger.LogWarning("⚠️ No se encontró token en los claims del usuario");
            return null;
        }

        /// <summary>
        /// Configura el header Authorization antes de cada petición
        /// </summary>
        private void ConfigurarHeaderDeAutorizacion()
        {
            var token = ObtenerTokenDelUsuarioActual();

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("🔐 Configurando header Authorization con Bearer token");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("⚠️ No hay token disponible para la petición");
            }
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
            {
                try
                {
                    _logger.LogInformation("🔍 Intentando login para: {Usuario}", request.NombreUsuario);
                    var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", request);

                    _logger.LogInformation("📊 Respuesta login - Status: {StatusCode}", response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                        _logger.LogInformation("✅ Login exitoso para: {Usuario}", request.NombreUsuario);
                        return result;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error en login: {Error}", errorContent);
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "❌ Error de conexión en login");
                    return null;
                }
            }

        public async Task<(bool Exito, List<string> Errores)> CambiarPasswordAsync(CambiarPasswordDto request)
        {
            var errores = new List<string>();
            try
            {
                ConfigurarHeaderDeAutorizacion();
                _logger.LogInformation("🔍 Intentando cambiar contraseña");
                var response = await _httpClient.PostAsJsonAsync("/api/usuarios/cambiar-password", request);

                _logger.LogInformation("📊 Respuesta cambiar password - Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Contraseña cambiada exitosamente");
                    return (true, errores);
                }

                var errorContent = await response.Content.ReadAsStringAsync();

                if (errorContent == "{\"error\":\"La contraseña actual es incorrecta.\"}")
                {
                    _logger.LogError("⚠️ Contraseña actual incorrecta");
                    errores.Add("⚠️ Contraseña actual incorrecta");
                }
                else
                {
                    _logger.LogError("❌ Error al cambiar contraseña: {Error}", errorContent);
                    errores.Add(errorContent);
                }
                return (false, errores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en cambiar contraseña");
                errores.Add($"Error: {ex.Message}");
                return (false, errores);
            }
        }

        public async Task<(bool Exito, List<string> Errores)> Insertar(SolicitudCrearUsuarioDto request)
        {
            try
            {
                ConfigurarHeaderDeAutorizacion();
                _logger.LogInformation("🔍 Iniciando registro de usuario: {Nombre} {ApellidoPaterno}", request.Nombre, request.ApellidoPaterno);

                // Asegurar formato de fecha para el backend (YYYY-MM-DD)
                string fechaNac = request.FechaNacimiento ?? "";
                if (DateTime.TryParse(fechaNac, out var fechaParsed))
                {
                    fechaNac = fechaParsed.ToString("yyyy-MM-dd");
                    _logger.LogDebug("📅 Fecha convertida: {Fecha}", fechaNac);
                }

                // El backend espera un objeto Usuario completo, agregamos campos técnicos faltantes
                var payload = new
                {
                    Nombre = request.Nombre,
                    ApellidoPaterno = request.ApellidoPaterno,
                    ApellidoMaterno = request.ApellidoMaterno,
                    Ci = request.Ci,
                    Complemento = request.Complemento,
                    Email = request.Email,
                    Telefono = request.Telefono,
                    Rol = request.Rol,
                    DireccionDomicilio = request.DireccionDomicilio ?? "Dirección no especificada",
                    FechaNacimiento = fechaNac,
                    FechaIngreso = DateTime.Now.ToString("yyyy-MM-dd"),
                    IdUsuario = 1 // ID por defecto del admin
                };

                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                _logger.LogInformation("📤 Enviando payload: {Payload}", jsonPayload);

                var response = await _httpClient.PostAsJsonAsync("/api/usuarios", payload);

                _logger.LogInformation("📊 Respuesta del servidor - Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Usuario registrado exitosamente");
                    return (true, new List<string>());
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Error en respuesta: {Content}", content);

                var errores = new List<string>();

                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    // 1. Intentar formato personalizado { "error": "..." }
                    if (root.TryGetProperty("error", out var errorProp))
                    {
                        errores.Add(errorProp.GetString() ?? "Error del servidor");
                    }
                    // 2. Intentar formato personalizado { "errores": [...] }
                    else if (root.TryGetProperty("errores", out var erroresProp) && erroresProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var err in erroresProp.EnumerateArray())
                            errores.Add(err.GetString() ?? "");
                    }
                    // 3. Intentar formato estándar ASP.NET { "errors": { "Campo": ["Error"] } }
                    else if (root.TryGetProperty("errors", out var validationErrors) && validationErrors.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var prop in validationErrors.EnumerateObject())
                        {
                            foreach (var err in prop.Value.EnumerateArray())
                                errores.Add($"{prop.Name}: {err.GetString()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "⚠️ Error al parsear JSON de error");
                    errores.Add("Error de validación en los datos. Verifique CI, Email y que sea mayor de 18 años.");
                }

                return (false, errores.Any() ? errores : new List<string> { "Error desconocido en el servidor." });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión HTTP: {Message}", ex.Message);
                _logger.LogError("❌ Inner Exception: {InnerException}", ex.InnerException?.Message);
                return (false, new List<string> { $"Error de conexión: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al insertar usuario: {Message}", ex.Message);
                return (false, new List<string> { $"Error crítico: {ex.Message}" });
            }
        }

        public async Task<List<UsuarioDto>> ObtenerTodos()
        {
            try
            {
                ConfigurarHeaderDeAutorizacion();
                _logger.LogInformation("🔍 Obteniendo todos los usuarios");
                var response = await _httpClient.GetAsync("/api/usuarios");

                _logger.LogInformation("📊 Respuesta ObtenerTodos - Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>() ?? new List<UsuarioDto>();
                    _logger.LogInformation("✅ Se obtuvieron {Count} usuarios", usuarios.Count);
                    return usuarios;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Error al obtener usuarios: {Error}", errorContent);
                return new List<UsuarioDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión al obtener usuarios");
                return new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener usuarios: {Message}", ex.Message);
                return new List<UsuarioDto>();
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                ConfigurarHeaderDeAutorizacion();
                _logger.LogInformation("🔍 Eliminando usuario con ID: {Id}", id);
                var response = await _httpClient.DeleteAsync($"/api/usuarios/{id}");

                _logger.LogInformation("📊 Respuesta Eliminar - Status: {StatusCode}", response.StatusCode);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("📋 Contenido respuesta: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Usuario eliminado: {Id}", id);
                    return true;
                }

                _logger.LogError("❌ Error al eliminar usuario {Id} - Status: {Status} - Respuesta: {Response}", id, response.StatusCode, responseContent);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión HTTP al eliminar usuario {Id}: {Message}", id, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar usuario {Id}: {Message}", id, ex.Message);
                return false;
            }
        }
    }
}
