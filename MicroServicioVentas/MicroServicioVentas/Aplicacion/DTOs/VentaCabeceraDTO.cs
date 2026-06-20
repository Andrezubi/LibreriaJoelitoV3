namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaCabeceraDTO
    {
        public int Id { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string EstadoVenta { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string? DocumentoCliente { get; set; }
        public string? NitCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public int IdUsuario { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
