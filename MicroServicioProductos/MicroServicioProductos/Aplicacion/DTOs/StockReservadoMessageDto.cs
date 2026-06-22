namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class StockReservadoMessageDto
    {
        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public int IdVenta { get; set; }
    }
}
