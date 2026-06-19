namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class ReservarStockMessageDto
    {
        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public int IdVenta { get; set; }

        public int IdUsuario { get; set; }

        public List<DetalleReservarStockMessageDto> Detalles { get; set; } = new();
    }
}