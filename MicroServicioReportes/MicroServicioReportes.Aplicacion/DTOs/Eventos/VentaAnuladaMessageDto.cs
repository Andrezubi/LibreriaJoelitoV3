namespace MicroServicioReportes.Aplicacion.DTOs.Eventos
{
    public class VentaAnuladaMessageDto
    {
        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public int VentaId { get; set; }

        public DateTime FechaAnulacion { get; set; }
    }
}