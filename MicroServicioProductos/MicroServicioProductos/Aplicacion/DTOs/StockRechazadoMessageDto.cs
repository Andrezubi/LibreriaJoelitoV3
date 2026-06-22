namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class StockRechazadoMessageDto
    {
        public string MessageId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public int IdVenta { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
