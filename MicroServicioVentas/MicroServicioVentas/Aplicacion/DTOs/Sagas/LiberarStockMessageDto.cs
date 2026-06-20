namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class LiberarStockMessageDto
    {
        public string MessageId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public List<DetalleLiberarStockMessageDto> Detalles { get; set; } = new();
    }
}
