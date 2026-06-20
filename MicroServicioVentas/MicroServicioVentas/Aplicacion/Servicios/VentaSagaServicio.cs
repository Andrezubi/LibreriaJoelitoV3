using System.Text.Json;
using Microsoft.Extensions.Options;
using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Mensajeria.Rabbit;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class VentaSagaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;
        private readonly ProcessedMessageRepositorio _processedMessageRepositorio;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public VentaSagaServicio(IOptions<RabbitMqOptions> rabbitMqOptions)
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
            _detalleVentaRepositorio = new DetalleVentaCreadorRepositorio().CrearRepositorio();
            _outboxMessageRepositorio = new OutboxMessageCreadorRepositorio().CrearRepositorio();
            _processedMessageRepositorio = new ProcessedMessageCreadorRepositorio().CrearRepositorio();
            _rabbitMqOptions = rabbitMqOptions.Value;
        }

        public Result ProcesarClienteValidado(ClienteValidadoMessageDto mensaje, string routingKey)
        {
            try
            {
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    if (_processedMessageRepositorio.Existe(mensaje.MessageId))
                    {
                        RepositorioBD.Instancia.Commit();
                        return Result.Success();
                    }

                    var venta = _ventaRepositorio.ObtenerPorCorrelationId(mensaje.CorrelationId);

                    if (venta == null)
                        throw new Exception("No se encontró la venta asociada al CorrelationId.");

                    if (venta.Estado == EstadosVenta.Pendiente)
                    {
                        _ventaRepositorio.ActualizarEstadoPorCorrelationId(
                            mensaje.CorrelationId,
                            EstadosVenta.ClienteValidado
                        );

                        var detalles = _detalleVentaRepositorio.ObtenerPorIdVenta(venta.Id);

                        string stockMessageId = Guid.NewGuid().ToString();

                        var reservarStockMessage = new ReservarStockMessageDto
                        {
                            MessageId = stockMessageId,
                            CorrelationId = mensaje.CorrelationId,
                            IdVenta = venta.Id,
                            IdUsuario = venta.IdUsuario,
                            Detalles = detalles.Select(d => new DetalleReservarStockMessageDto
                            {
                                IdProducto = d.IdProducto,
                                IdPresentacion = d.IdPresentacion,
                                Cantidad = d.Cantidad
                            }).ToList()
                        };

                        string payload = JsonSerializer.Serialize(reservarStockMessage);

                        var outboxMessage = new OutboxMessage(
                            messageId: stockMessageId,
                            correlationId: mensaje.CorrelationId,
                            exchangeName: _rabbitMqOptions.ExchangeName,
                            routingKey: _rabbitMqOptions.RoutingKeys.StockReservar,
                            messageType: nameof(ReservarStockMessageDto),
                            payload: payload
                        );

                        int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

                        if (filasOutbox <= 0)
                            throw new Exception("No se pudo registrar el mensaje Outbox para reservar stock.");
                    }

                    _processedMessageRepositorio.Insertar(new ProcessedMessage(
                        mensaje.MessageId,
                        mensaje.CorrelationId,
                        routingKey
                    ));

                    RepositorioBD.Instancia.Commit();

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result.Failure(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al procesar cliente.validado: {ex.Message}");
            }
        }

        public Result ProcesarClienteRechazado(ClienteRechazadoMessageDto mensaje, string routingKey)
        {
            try
            {
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    if (_processedMessageRepositorio.Existe(mensaje.MessageId))
                    {
                        RepositorioBD.Instancia.Commit();
                        return Result.Success();
                    }

                    var venta = _ventaRepositorio.ObtenerPorCorrelationId(mensaje.CorrelationId);

                    if (venta == null)
                        throw new Exception("No se encontró la venta asociada al CorrelationId.");

                    if (venta.Estado == EstadosVenta.Pendiente)
                    {
                        _ventaRepositorio.ActualizarEstadoPorCorrelationId(
                            mensaje.CorrelationId,
                            EstadosVenta.ClienteRechazado,
                            mensaje.Motivo
                        );

                        _detalleVentaRepositorio.ActualizarEstadoPorVenta(
                            venta.Id,
                            EstadosDetalleVenta.Fallido
                        );
                    }

                    _processedMessageRepositorio.Insertar(new ProcessedMessage(
                        mensaje.MessageId,
                        mensaje.CorrelationId,
                        routingKey
                    ));

                    RepositorioBD.Instancia.Commit();

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result.Failure(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al procesar cliente.rechazado: {ex.Message}");
            }
        }

        public Result ProcesarStockReservado(StockReservadoMessageDto mensaje, string routingKey)
        {
            try
            {
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    if (_processedMessageRepositorio.Existe(mensaje.MessageId))
                    {
                        RepositorioBD.Instancia.Commit();
                        return Result.Success();
                    }

                    var venta = _ventaRepositorio.ObtenerPorCorrelationId(mensaje.CorrelationId);

                    if (venta == null)
                        throw new Exception("No se encontró la venta asociada al CorrelationId.");

                    if (venta.Estado == EstadosVenta.ClienteValidado ||
                        venta.Estado == EstadosVenta.StockReservado)
                    {
                        _ventaRepositorio.ActualizarEstadoPorCorrelationId(
                            mensaje.CorrelationId,
                            EstadosVenta.Confirmada
                        );

                        _detalleVentaRepositorio.ActualizarEstadoPorVenta(
                            venta.Id,
                            EstadosDetalleVenta.Confirmado
                        );
                    }

                    _processedMessageRepositorio.Insertar(new ProcessedMessage(
                        mensaje.MessageId,
                        mensaje.CorrelationId,
                        routingKey
                    ));

                    RepositorioBD.Instancia.Commit();

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result.Failure(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al procesar stock.reservado: {ex.Message}");
            }
        }

        public Result ProcesarStockRechazado(StockRechazadoMessageDto mensaje, string routingKey)
        {
            try
            {
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    if (_processedMessageRepositorio.Existe(mensaje.MessageId))
                    {
                        RepositorioBD.Instancia.Commit();
                        return Result.Success();
                    }

                    var venta = _ventaRepositorio.ObtenerPorCorrelationId(mensaje.CorrelationId);

                    if (venta == null)
                        throw new Exception("No se encontró la venta asociada al CorrelationId.");

                    if (venta.Estado == EstadosVenta.ClienteValidado ||
                        venta.Estado == EstadosVenta.StockReservado)
                    {
                        _ventaRepositorio.ActualizarEstadoPorCorrelationId(
                            mensaje.CorrelationId,
                            EstadosVenta.StockRechazado,
                            mensaje.Motivo
                        );

                        _detalleVentaRepositorio.ActualizarEstadoPorVenta(
                            venta.Id,
                            EstadosDetalleVenta.Fallido
                        );
                    }

                    _processedMessageRepositorio.Insertar(new ProcessedMessage(
                        mensaje.MessageId,
                        mensaje.CorrelationId,
                        routingKey
                    ));

                    RepositorioBD.Instancia.Commit();

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result.Failure(ex.Message);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error al procesar stock.rechazado: {ex.Message}");
            }
        }
    }
}