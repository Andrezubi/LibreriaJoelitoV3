using MicroServicioReportes.Aplicacion.DTOs.Eventos;
using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Infraestructura.Mensajeria.Rabbit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace MicroServicioReportes.Infraestructura.Mensajeria.Consumers
{
    public class ReportesSagaConsumerService : BackgroundService
    {
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportesSagaConsumerService> _logger;

        private IConnection? _connection;
        private IModel? _channel;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReportesSagaConsumerService(
            IOptions<RabbitMqOptions> options,
            IServiceScopeFactory scopeFactory,
            ILogger<ReportesSagaConsumerService> logger)
        {
            _options = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InicializarRabbitMq();

            var consumer = new AsyncEventingBasicConsumer(_channel!);

            consumer.Received += async (_, eventArgs) =>
            {
                await ProcesarMensaje(eventArgs);
            };

            _channel!.BasicConsume(
                queue: _options.QueueNames.ReportesVentas,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogInformation(
                "Consumer de Reportes iniciado. Cola: {Queue}",
                _options.QueueNames.ReportesVentas
            );

            return Task.CompletedTask;
        }

        private void InicializarRabbitMq()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueDeclare(
                queue: _options.QueueNames.ReportesVentas,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBind(
                queue: _options.QueueNames.ReportesVentas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.VentaConfirmada
            );

            _channel.QueueBind(
                queue: _options.QueueNames.ReportesVentas,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeys.VentaAnulada
            );

            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            _logger.LogInformation(
                "RabbitMQ configurado para Reportes. Exchange: {Exchange}, Queue: {Queue}",
                _options.ExchangeName,
                _options.QueueNames.ReportesVentas
            );
        }

        private async Task ProcesarMensaje(BasicDeliverEventArgs eventArgs)
        {
            string routingKey = eventArgs.RoutingKey;
            string json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                _logger.LogInformation(
                    "Mensaje recibido en Reportes. RoutingKey: {RoutingKey}. Payload: {Payload}",
                    routingKey,
                    json
                );

                using var scope = _scopeFactory.CreateScope();

                var sagaServicio = scope.ServiceProvider
                    .GetRequiredService<IComprobanteVentaSagaServicio>();

                if (routingKey == _options.RoutingKeys.VentaConfirmada)
                {
                    var evento = JsonSerializer.Deserialize<VentaConfirmadaMessageDto>(
                        json,
                        _jsonOptions
                    );

                    if (evento == null)
                        throw new InvalidOperationException("No se pudo deserializar VentaConfirmadaMessageDto.");

                    sagaServicio.ProcesarVentaConfirmada(evento);
                }
                else if (routingKey == _options.RoutingKeys.VentaAnulada)
                {
                    var evento = JsonSerializer.Deserialize<VentaAnuladaMessageDto>(
                        json,
                        _jsonOptions
                    );

                    if (evento == null)
                        throw new InvalidOperationException("No se pudo deserializar VentaAnuladaMessageDto.");

                    sagaServicio.ProcesarVentaAnulada(evento);
                }
                else
                {
                    _logger.LogWarning(
                        "RoutingKey no manejada por Reportes: {RoutingKey}",
                        routingKey
                    );
                }

                _channel!.BasicAck(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false
                );

                _logger.LogInformation(
                    "Mensaje procesado correctamente. RoutingKey: {RoutingKey}",
                    routingKey
                );
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Error de formato JSON. El mensaje será rechazado sin reencolarse. Payload: {Payload}",
                    json
                );

                _channel!.BasicNack(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error procesando mensaje de Reportes. El mensaje será reencolado. RoutingKey: {RoutingKey}",
                    routingKey
                );

                _channel!.BasicNack(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true
                );
            }

            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}