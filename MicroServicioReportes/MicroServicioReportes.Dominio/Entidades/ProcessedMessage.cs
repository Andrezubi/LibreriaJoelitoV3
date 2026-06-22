namespace MicroServicioReportes.Dominio.Entidades;

public class ProcessedMessage
{
    public int Id { get; set; }

    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; } = DateTime.Now;
}