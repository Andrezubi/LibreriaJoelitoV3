using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Mensajeria.Rabbit;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.Mensajeria.Outbox
{
    public class OutboxPublisherService : BackgroundService
    {
        private readonly ILogger<OutboxPublisherService> _logger;
        private readonly RabbitPublisher _rabbitPublisher;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;

        public OutboxPublisherService(
            ILogger<OutboxPublisherService> logger,
            RabbitPublisher rabbitPublisher)
        {
            _logger = logger;
            _rabbitPublisher = rabbitPublisher;
            _outboxMessageRepositorio = new OutboxMessageCreadorRepositorio().CrearRepositorio();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxPublisherService iniciado.");

            try
            {
                await _rabbitPublisher.DeclararExchangeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo declarar el exchange inicial de RabbitMQ.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var mensajesPendientes = _outboxMessageRepositorio.ObtenerPendientes(20);

                    foreach (var mensaje in mensajesPendientes)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            await _rabbitPublisher.PublicarAsync(
                                exchangeName: mensaje.ExchangeName,
                                routingKey: mensaje.RoutingKey,
                                payload: mensaje.Payload,
                                cancellationToken: stoppingToken
                            );

                            _outboxMessageRepositorio.MarcarComoPublicado(mensaje.MessageId);

                            _logger.LogInformation(
                                "Mensaje Outbox publicado correctamente. MessageId: {MessageId}, RoutingKey: {RoutingKey}",
                                mensaje.MessageId,
                                mensaje.RoutingKey
                            );
                        }
                        catch (Exception ex)
                        {
                            _outboxMessageRepositorio.MarcarComoFallido(
                                mensaje.MessageId,
                                ex.Message
                            );

                            _logger.LogWarning(
                                ex,
                                "No se pudo publicar mensaje Outbox. MessageId: {MessageId}, RoutingKey: {RoutingKey}",
                                mensaje.MessageId,
                                mensaje.RoutingKey
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error general ejecutando OutboxPublisherService.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("OutboxPublisherService detenido.");
        }
    }
}