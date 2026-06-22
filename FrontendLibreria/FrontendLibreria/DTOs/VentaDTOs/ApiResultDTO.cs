namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class ApiResultDTO<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; set; }
        public string? Error { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}