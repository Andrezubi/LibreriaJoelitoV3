namespace MicroServicioProductos.Dominio.Modelos
{
    public class ProcessedMessage
    {
        public string MessageId { get; set; } = string.Empty;

        public string? CorrelationId { get; set; }

        public string? RoutingKey { get; set; }

        public DateTime ProcessedAt { get; set; }

        public ProcessedMessage()
        {
        }

        public ProcessedMessage(string messageId, string? correlationId, string? routingKey)
        {
            MessageId = messageId;
            CorrelationId = correlationId;
            RoutingKey = routingKey;
        }
    }
}
