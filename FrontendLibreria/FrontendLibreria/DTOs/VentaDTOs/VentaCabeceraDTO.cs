namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class VentaCabeceraDTO
    {
        public int Id { get; set; }
        public int EstadoVenta { get; set; }
        public int CiCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}