using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Aplicacion.Servicios;
using MicroServicioVentas.Infraestructura.Mensajeria.Rabbit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroServicioVentas.Infraestructura.Mensajeria.Consumers
{
    public class VentasSagaConsumerService : BackgroundService
    {
        private readonly ILogger<VentasSagaConsumerService> _logger;
        private readonly RabbitMqOptions _options;
        private readonly VentaSagaServicio _ventaSagaServicio;

        private IConnection? _connection;
        private IChannel? _channel;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public VentasSagaConsumerService(
            ILogger<VentasSagaConsumerService> logger,
            IOptions<RabbitMqOptions> options,
            VentaSagaServicio ventaSagaServicio)
        {
            _logger = logger;
            _options = options.Value;
            _ventaSagaServicio = ventaSagaServicio;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VentasSagaConsumerService iniciado.");

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = "/",
                ClientProvidedName = "MicroServicioVentas.Consumer"
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueDeclareAsync(
                queue: _options.QueueNames.VentasRespuestas,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _options.QueueNames.VentasRespuestas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.ClienteValidado,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _options.QueueNames.VentasRespuestas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.ClienteRechazado,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _options.QueueNames.VentasRespuestas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.StockReservado,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _options.QueueNames.VentasRespuestas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.StockRechazado,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += ProcesarMensajeAsync;

            await _channel.BasicConsumeAsync(
                queue: _options.QueueNames.VentasRespuestas,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation(
                "Ventas escuchando respuestas de saga en cola {Queue}.",
                _options.QueueNames.VentasRespuestas
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcesarMensajeAsync(object sender, BasicDeliverEventArgs ea)
        {
            if (_channel == null)
                return;

            string routingKey = ea.RoutingKey;
            string payload = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                _logger.LogInformation(
                    "Mensaje recibido en Ventas. RoutingKey: {RoutingKey}. Payload: {Payload}",
                    routingKey,
                    payload
                );

                Result resultado;

                if (routingKey == _options.RoutingKeys.ClienteValidado)
                {
                    var mensaje = JsonSerializer.Deserialize<ClienteValidadoMessageDto>(
                        payload,
                        _jsonOptions
                    );

                    if (mensaje == null)
                        throw new Exception("No se pudo deserializar ClienteValidadoMessageDto.");

                    resultado = _ventaSagaServicio.ProcesarClienteValidado(mensaje, routingKey);
                }
                else if (routingKey == _options.RoutingKeys.ClienteRechazado)
                {
                    var mensaje = JsonSerializer.Deserialize<ClienteRechazadoMessageDto>(
                        payload,
                        _jsonOptions
                    );

                    if (mensaje == null)
                        throw new Exception("No se pudo deserializar ClienteRechazadoMessageDto.");

                    resultado = _ventaSagaServicio.ProcesarClienteRechazado(mensaje, routingKey);
                }
                else if (routingKey == _options.RoutingKeys.StockReservado)
                {
                    var mensaje = JsonSerializer.Deserialize<StockReservadoMessageDto>(
                        payload,
                        _jsonOptions
                    );

                    if (mensaje == null)
                        throw new Exception("No se pudo deserializar StockReservadoMessageDto.");

                    resultado = _ventaSagaServicio.ProcesarStockReservado(mensaje, routingKey);
                }
                else if (routingKey == _options.RoutingKeys.StockRechazado)
                {
                    var mensaje = JsonSerializer.Deserialize<StockRechazadoMessageDto>(
                        payload,
                        _jsonOptions
                    );

                    if (mensaje == null)
                        throw new Exception("No se pudo deserializar StockRechazadoMessageDto.");

                    resultado = _ventaSagaServicio.ProcesarStockRechazado(mensaje, routingKey);
                }
                else
                {
                    _logger.LogWarning(
                        "RoutingKey no manejada por Ventas: {RoutingKey}",
                        routingKey
                    );

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
                }

                if (resultado.IsSuccess)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                    _logger.LogInformation(
                        "Mensaje procesado correctamente en Ventas. RoutingKey: {RoutingKey}",
                        routingKey
                    );
                }
                else
                {
                    await _channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );

                    _logger.LogWarning(
                        "Mensaje rechazado por error de negocio. RoutingKey: {RoutingKey}. Errores: {Errores}",
                        routingKey,
                        string.Join(" | ", resultado.Errors)
                    );
                }
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false
                );

                _logger.LogError(
                    ex,
                    "Error procesando mensaje en Ventas. RoutingKey: {RoutingKey}",
                    routingKey
                );
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}