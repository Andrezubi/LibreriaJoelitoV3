using Microsoft.AspNetCore.Http;

namespace FrontendLibreria.DTOs
{
    public class ResultadoApi
    {
        public bool Success { get; private set; }
        public List<string> Errors { get; private set; } = new();

        public static ResultadoApi Ok() => new() { Success = true };
        public static ResultadoApi Fail(List<string> errors) => new() { Success = false, Errors = errors };
    }
}
