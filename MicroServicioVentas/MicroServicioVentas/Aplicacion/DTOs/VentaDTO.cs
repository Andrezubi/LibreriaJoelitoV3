namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
