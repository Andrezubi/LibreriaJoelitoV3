using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MicroServicioProductos.Aplicacion.DTOs;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Mensajeria.Rabbit;
using MicroServicioProductos.Infraestructura.Persistencia;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroServicioProductos.Infraestructura.Mensajeria.Consumers;

public class ProductoSagaConsumerServicio : BackgroundService
{
    private readonly ILogger<ProductoSagaConsumerServicio> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    public ProductoSagaConsumerServicio(
        ILogger<ProductoSagaConsumerServicio> logger,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _options.QueueNames.ProductosComandos,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: _options.QueueNames.ProductosComandos,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKeys.StockReservar,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: _options.QueueNames.ProductosComandos,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKeys.StockLiberar,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += ProcesarMensaje;

        await _channel.BasicConsumeAsync(
            queue: _options.QueueNames.ProductosComandos,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcesarMensaje(object sender, BasicDeliverEventArgs ea)
    {
        string routingKey = ea.RoutingKey;
        string payload = Encoding.UTF8.GetString(ea.Body.ToArray());

        // Creamos un scope nuevo por cada mensaje procesado.
        // Aquí adentro sí podemos resolver servicios Scoped sin problema.
        using var scope = _scopeFactory.CreateScope();

        var productoRepositorio = scope.ServiceProvider.GetRequiredService<ProductoRepositorio>();
        var presentacionRepositorio = scope.ServiceProvider.GetRequiredService<PresentacionProductoRepositorio>();
        var outboxRepositorio = scope.ServiceProvider.GetRequiredService<OutboxMessageRepositorio>();
        var processedRepositorio = scope.ServiceProvider.GetRequiredService<ProcessedMessageRepositorio>();

        try
        {
            if (routingKey == _options.RoutingKeys.StockReservar)
            {
                var mensaje = JsonSerializer.Deserialize<ReservarStockMessageDto>(payload);

                ProcesarReserva(
                    mensaje!,
                    productoRepositorio,
                    presentacionRepositorio,
                    outboxRepositorio,
                    processedRepositorio);
            }
            else if (routingKey == _options.RoutingKeys.StockLiberar)
            {
                var mensaje = JsonSerializer.Deserialize<LiberarStockMessageDto>(payload);

                ProcesarLiberacion(
                    mensaje!,
                    presentacionRepositorio,
                    productoRepositorio,
                    outboxRepositorio,
                    processedRepositorio);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saga productos");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    private void ProcesarReserva(
        ReservarStockMessageDto mensaje,
        ProductoRepositorio productoRepositorio,
        PresentacionProductoRepositorio presentacionRepositorio,
        OutboxMessageRepositorio outboxRepositorio,
        ProcessedMessageRepositorio processedRepositorio)
    {
        RepositorioBD.Instancia.BeginTransaction();
        try
        {
            if (processedRepositorio.Existe(mensaje.MessageId))
            {
                RepositorioBD.Instancia.Commit();
                return;
            }

            // PRIMERA PASADA: VALIDAR STOCK
            foreach (var detalle in mensaje.Detalles)
            {
                var presentacion = presentacionRepositorio.ObtenerEntidadPorIds(
                    detalle.IdProducto, detalle.IdPresentacion);

                if (presentacion == null)
                    throw new Exception(
                        $"No existe relación producto-presentación {detalle.IdProducto} - {detalle.IdPresentacion}");

                int cantidadReal = detalle.Cantidad * presentacion.FactorConversion;

                var producto = productoRepositorio.ObtenerPorId(detalle.IdProducto);

                if (producto == null)
                    throw new Exception("Producto inexistente " + detalle.IdProducto);

                if (producto.Stock < cantidadReal)
                {
                    CrearEventoRechazado(mensaje, "Stock insuficiente para " + producto.Nombre, outboxRepositorio);
                    GuardarMensajeProcesado(mensaje, _options.RoutingKeys.StockReservar, processedRepositorio);
                    RepositorioBD.Instancia.Commit();
                    return;
                }
            }

            // SEGUNDA PASADA: DESCONTAR STOCK
            foreach (var detalle in mensaje.Detalles)
            {
                var presentacion = presentacionRepositorio.ObtenerEntidadPorIds(
                    detalle.IdProducto, detalle.IdPresentacion);

                int cantidadReal = detalle.Cantidad * presentacion!.FactorConversion;

                int filas = productoRepositorio.DescontarStock(detalle.IdProducto, cantidadReal);

                if (filas == 0)
                    throw new Exception("No se pudo descontar stock");
            }

            CrearEventoReservado(mensaje, outboxRepositorio);
            GuardarMensajeProcesado(mensaje, _options.RoutingKeys.StockReservar, processedRepositorio);

            RepositorioBD.Instancia.Commit();
        }
        catch
        {
            RepositorioBD.Instancia.Rollback();
            throw;
        }
    }

    private void ProcesarLiberacion(
        LiberarStockMessageDto mensaje,
        PresentacionProductoRepositorio presentacionRepositorio,
        ProductoRepositorio productoRepositorio,
        OutboxMessageRepositorio outboxRepositorio,
        ProcessedMessageRepositorio processedRepositorio)
    {
        RepositorioBD.Instancia.BeginTransaction();
        try
        {
            foreach (var detalle in mensaje.Detalles)
            {
                var presentacion = presentacionRepositorio.ObtenerEntidadPorIds(
                    detalle.IdProducto, detalle.IdPresentacion);

                int cantidadReal = detalle.Cantidad * presentacion!.FactorConversion;

                productoRepositorio.RestaurarStock(detalle.IdProducto, cantidadReal);
            }

            CrearEventoLiberado(mensaje, outboxRepositorio);
            GuardarMensajeProcesado(mensaje, _options.RoutingKeys.StockLiberar, processedRepositorio);

            RepositorioBD.Instancia.Commit();
        }
        catch
        {
            RepositorioBD.Instancia.Rollback();
            throw;
        }
    }

    private void GuardarMensajeProcesado(
        dynamic mensaje,
        string routingKey,
        ProcessedMessageRepositorio processedRepositorio)
    {
        processedRepositorio.Insertar(
            new ProcessedMessage(mensaje.MessageId, mensaje.CorrelationId, routingKey));
    }

    private void CrearEventoRechazado(
        ReservarStockMessageDto mensaje,
        string motivo,
        OutboxMessageRepositorio outboxRepositorio)
    {
        var dto = new StockRechazadoMessageDto
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = mensaje.CorrelationId,
            IdVenta = mensaje.IdVenta,
            Motivo = motivo
        };

        string payload = JsonSerializer.Serialize(dto);

        var outbox = new OutboxMessage(
            dto.MessageId,
            dto.CorrelationId,
            _options.ExchangeName,
            _options.RoutingKeys.StockRechazado,
            nameof(StockRechazadoMessageDto),
            payload);

        outboxRepositorio.Insertar(outbox);
    }

    private void CrearEventoLiberado(
        LiberarStockMessageDto mensaje,
        OutboxMessageRepositorio outboxRepositorio)
    {
        var dto = new StockLiberadoMessageDto
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = mensaje.CorrelationId,
            IdVenta = mensaje.IdVenta
        };

        string payload = JsonSerializer.Serialize(dto);

        var outbox = new OutboxMessage(
            dto.MessageId,
            dto.CorrelationId,
            _options.ExchangeName,
            _options.RoutingKeys.StockLiberado,
            nameof(StockLiberadoMessageDto),
            payload);

        outboxRepositorio.Insertar(outbox);
    }

    private void CrearEventoReservado(
        ReservarStockMessageDto mensaje,
        OutboxMessageRepositorio outboxRepositorio)
    {
        var dto = new StockReservadoMessageDto
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = mensaje.CorrelationId,
            IdVenta = mensaje.IdVenta
        };

        string payload = JsonSerializer.Serialize(dto);

        var outbox = new OutboxMessage(
            dto.MessageId,
            dto.CorrelationId,
            _options.ExchangeName,
            _options.RoutingKeys.StockReservado,
            nameof(StockReservadoMessageDto),
            payload);

        outboxRepositorio.Insertar(outbox);
    }
}