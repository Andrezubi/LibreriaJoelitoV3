namespace MicroServicioReportes.Infraestructura.Mensajeria.Rabbit;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "saga.exchange";

    public RabbitMqRoutingKeys RoutingKeys { get; set; } = new();
    public RabbitMqQueueNames QueueNames { get; set; } = new();
}

public class RabbitMqRoutingKeys
{
    public string VentaConfirmada { get; set; } = "venta.confirmada";
    public string VentaAnulada { get; set; } = "venta.anulada";
}

public class RabbitMqQueueNames
{
    public string ReportesVentas { get; set; } = "reportes.saga.ventas";
}