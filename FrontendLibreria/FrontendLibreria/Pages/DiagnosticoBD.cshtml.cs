using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendLibreria.Pages
{
    public class DiagnosticoBDModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticoBDModel> _logger;

        public string MicroservicioUrl { get; set; }
        public string BaseUrl { get; set; }
        public string Ambiente { get; set; }
        public ResultadoPrueba ResultadoPrueba { get; set; }

        public DiagnosticoBDModel(HttpClient httpClient, IConfiguration configuration, ILogger<DiagnosticoBDModel> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
            MicroservicioUrl = _configuration["ApiSettings:MicroServicioUsuariosUrl"];
            BaseUrl = _httpClient.BaseAddress?.ToString() ?? "No configurada";
            Ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        }

        public async Task OnPostProbearConexion()
        {
            MicroservicioUrl = _configuration["ApiSettings:MicroServicioUsuariosUrl"];
            BaseUrl = _httpClient.BaseAddress?.ToString() ?? "No configurada";
            Ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            try
            {
                _logger.LogInformation("🔍 Iniciando diagnóstico de conexión");
                _logger.LogInformation("📍 URL: {Url}", MicroservicioUrl);
                _logger.LogInformation("📍 Base Address: {Base}", BaseUrl);

                // Intentar obtener usuarios
                var url = $"{MicroservicioUrl.TrimEnd('/')}/api/usuarios";
                _logger.LogInformation("📡 Llamando a: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                _logger.LogInformation("📊 Status Code: {Code}", response.StatusCode);

                ResultadoPrueba = new ResultadoPrueba
                {
                    Exitoso = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode.ToString(),
                    Mensaje = response.IsSuccessStatusCode 
                        ? "Conexión exitosa al microservicio" 
                        : $"Error: {response.ReasonPhrase}"
                };

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ResultadoPrueba.Datos = content;
                    _logger.LogInformation("✅ Datos recibidos: {Data}", content);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ResultadoPrueba.DetalleError = errorContent;
                    _logger.LogError("❌ Error: {Error}", errorContent);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión HTTP");
                ResultadoPrueba = new ResultadoPrueba
                {
                    Exitoso = false,
                    StatusCode = "Error",
                    Mensaje = "Error de conexión",
                    DetalleError = $"HttpRequestException: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado");
                ResultadoPrueba = new ResultadoPrueba
                {
                    Exitoso = false,
                    StatusCode = "Error",
                    Mensaje = "Error inesperado",
                    DetalleError = $"{ex.GetType().Name}: {ex.Message}\n\nStackTrace: {ex.StackTrace}"
                };
            }
        }
    }

    public class ResultadoPrueba
    {
        public bool Exitoso { get; set; }
        public string StatusCode { get; set; }
        public string Mensaje { get; set; }
        public string DetalleError { get; set; }
        public string Datos { get; set; }
    }
}
