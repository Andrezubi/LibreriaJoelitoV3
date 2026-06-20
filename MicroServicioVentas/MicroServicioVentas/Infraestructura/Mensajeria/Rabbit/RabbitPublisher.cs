using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MicroServicioVentas.Infraestructura.Mensajeria.Rabbit
{
    public class RabbitPublisher
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitPublisher> _logger;

        public RabbitPublisher(
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task DeclararExchangeAsync(CancellationToken cancellationToken = default)
        {
            var factory = CrearConnectionFactory();

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Exchange declarado en RabbitMQ. Host: {Host}, Port: {Port}, Exchange: {Exchange}",
                _options.HostName,
                _options.Port,
                _options.ExchangeName
            );
        }

        public async Task PublicarAsync(
            string exchangeName,
            string routingKey,
            string payload,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                exchangeName = _options.ExchangeName;

            var factory = CrearConnectionFactory();

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Declarando exchange RabbitMQ. Host: {Host}, Port: {Port}, Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                _options.HostName,
                _options.Port,
                exchangeName,
                routingKey
            );

            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );

            byte[] body = Encoding.UTF8.GetBytes(payload);

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                body: body,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Mensaje publicado en RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                exchangeName,
                routingKey
            );
        }

        private ConnectionFactory CrearConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = "/",
                ClientProvidedName = "MicroServicioVentas"
            };
        }
    }
}