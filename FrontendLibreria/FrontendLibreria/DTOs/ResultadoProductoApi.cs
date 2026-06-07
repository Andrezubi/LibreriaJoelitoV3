using Microsoft.AspNetCore.Http;

namespace FrontendLibreria.DTOs
{
    public class ErrorValidacionDto
    {
        public string Campo { get; set; } = "";
        public string Mensaje { get; set; } = "";
    }

    public class ResultadoProductoApi
    {
        public bool Success { get; private set; }

        public List<ErrorValidacionDto> Errors { get; private set; } = new();

        public static ResultadoProductoApi Ok() => new() { Success = true };

        public static ResultadoProductoApi Fail(List<ErrorValidacionDto> errors) => new() {
            Success = false,
            Errors = errors
        };
    }
}
