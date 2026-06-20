namespace MicroServicioVentas.Infraestructura.Mensajeria.Rabbit
{
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
        public string ClienteValidar { get; set; } = "cliente.validar";

        public string ClienteValidado { get; set; } = "cliente.validado";

        public string ClienteRechazado { get; set; } = "cliente.rechazado";

        public string StockReservar { get; set; } = "stock.reservar";

        public string StockReservado { get; set; } = "stock.reservado";

        public string StockRechazado { get; set; } = "stock.rechazado";
    }

    public class RabbitMqQueueNames
    {
        public string VentasRespuestas { get; set; } = "ventas.saga.respuestas";
    }
}