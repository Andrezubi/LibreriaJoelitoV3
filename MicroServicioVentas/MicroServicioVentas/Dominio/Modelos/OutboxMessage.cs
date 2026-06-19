using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Dominio.Modelos
{
    public class OutboxMessage
    {
        public long Id { get; set; }

        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public string ExchangeName { get; set; } = "saga.exchange";

        public string RoutingKey { get; set; } = string.Empty;

        public string MessageType { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public string Status { get; set; } = EstadosOutboxMessage.Pending;

        public int RetryCount { get; set; }

        public string? LastError { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? LastAttemptAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public OutboxMessage()
        {
            MessageId = Guid.NewGuid().ToString();
            Status = EstadosOutboxMessage.Pending;
        }

        public OutboxMessage(
            string correlationId,
            string routingKey,
            string messageType,
            string payload)
        {
            MessageId = Guid.NewGuid().ToString();
            CorrelationId = correlationId;
            RoutingKey = routingKey;
            MessageType = messageType;
            Payload = payload;
            ExchangeName = "saga.exchange";
            Status = EstadosOutboxMessage.Pending;
        }

        public void MarcarComoPublicado()
        {
            Status = EstadosOutboxMessage.Published;
            LastError = null;
        }

        public void MarcarComoFallido(string error)
        {
            Status = EstadosOutboxMessage.Failed;
            RetryCount++;
            LastError = error;
        }

        public void MarcarComoPendiente()
        {
            Status = EstadosOutboxMessage.Pending;
        }
    }
}