namespace MicroServicioProductos.Aplicacion.DTOs
{
	public class ReservarStockMessageDto
    {

        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public List<DetalleReservarStockMessageDto> Detalles { get; set; }
    }
}
