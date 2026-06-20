namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class StockReservadoMessageDto
    {
        public string MessageId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public int IdVenta { get; set; }
    }
}
