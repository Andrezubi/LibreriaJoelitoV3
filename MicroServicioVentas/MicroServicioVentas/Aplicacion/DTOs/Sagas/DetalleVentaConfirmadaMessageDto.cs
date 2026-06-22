using System.Text.Json.Serialization;

namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class DetalleVentaConfirmadaMessageDto
    {
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("productoNombre")]
        public string ProductoNombre { get; set; } = string.Empty;

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("precioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }
    }
}