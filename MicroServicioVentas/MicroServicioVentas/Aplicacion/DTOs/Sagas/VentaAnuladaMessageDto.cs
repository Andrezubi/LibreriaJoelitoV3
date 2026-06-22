using System.Text.Json.Serialization;

namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class VentaAnuladaMessageDto
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("ventaId")]
        public int VentaId { get; set; }

        [JsonPropertyName("fechaAnulacion")]
        public DateTime FechaAnulacion { get; set; }
    }
}