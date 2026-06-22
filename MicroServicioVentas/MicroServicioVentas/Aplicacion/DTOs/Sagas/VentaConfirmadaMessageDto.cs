using System.Text.Json.Serialization;

namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class VentaConfirmadaMessageDto
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("ventaId")]
        public int VentaId { get; set; }

        [JsonPropertyName("clienteId")]
        public int ClienteId { get; set; }

        [JsonPropertyName("clienteNombre")]
        public string ClienteNombre { get; set; } = string.Empty;

        [JsonPropertyName("clienteCi")]
        public string ClienteCiNit { get; set; } = string.Empty;

        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("usuarioNombre")]
        public string UsuarioNombre { get; set; } = string.Empty;

        [JsonPropertyName("fechaVenta")]
        public DateTime FechaVenta { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("detalles")]
        public List<DetalleVentaConfirmadaMessageDto> Detalles { get; set; } = new();
    }
}