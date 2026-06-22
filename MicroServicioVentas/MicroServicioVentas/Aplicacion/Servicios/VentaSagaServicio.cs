using System.Text.Json;
using Microsoft.Extensions.Options;
using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;
using MicroServicioVentas.Infraestructura.Mensajeria.Rabbit;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class VentaSagaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly VentaClienteSnapshotRepositorio _clienteSnapshotRepositorio;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;
        private readonly ProcessedMessageRepositorio _processedMessageRepositorio;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public VentaSagaServicio(
            VentaRepositorio ventaRepositorio,
            DetalleVentaRepositorio detalleVentaRepositorio,
            VentaClienteSnapshotRepositorio clienteSnapshotRepositorio,
            OutboxMessageRepositorio outboxMessageRepositorio,
            ProcessedMessageRepositorio processedMessageRepositorio,
            IOptions<RabbitMqOptions> rabbitMqOptions)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _clienteSnapshotRepositorio = clienteSnapshotRepositorio;
            _outboxMessageRepositorio = outboxMessageRepositorio;
            _processedMessageRepositorio = processedMessageRepositorio;
            _rabbitMqOptions = rabbitMqOptions.Value;
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

                    if (venta.Estado == EstadosVenta.Pendiente ||
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

                        CrearEventoVentaConfirmadaEnOutbox(venta);
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

                    if (venta.Estado == EstadosVenta.Pendiente ||
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

        public Result ProcesarStockLiberado(StockLiberadoMessageDto mensaje, string routingKey)
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

                    if (venta.Estado == EstadosVenta.AnulacionPendiente)
                    {
                        _ventaRepositorio.ActualizarEstadoPorCorrelationId(
                            mensaje.CorrelationId,
                            EstadosVenta.Anulada
                        );

                        _detalleVentaRepositorio.ActualizarEstadoPorVenta(
                            venta.Id,
                            EstadosDetalleVenta.Liberado
                        );

                        CrearEventoVentaAnuladaEnOutbox(venta);
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
                return Result.Failure($"Error al procesar stock.liberado: {ex.Message}");
            }
        }

        private void CrearEventoVentaConfirmadaEnOutbox(Venta venta)
        {
            var clienteSnapshot = _clienteSnapshotRepositorio.ObtenerPorIdVenta(venta.Id);

            if (clienteSnapshot == null)
                throw new Exception("No se encontró el snapshot del cliente para publicar venta.confirmada.");

            var detalles = _detalleVentaRepositorio.ObtenerPorIdVenta(venta.Id);

            if (detalles == null || !detalles.Any())
                throw new Exception("No se encontraron detalles de venta para publicar venta.confirmada.");

            string messageId = Guid.NewGuid().ToString();

            var evento = new VentaConfirmadaMessageDto
            {
                MessageId = messageId,
                CorrelationId = venta.CorrelationId,
                VentaId = venta.Id,
                ClienteId = venta.IdCliente,
                ClienteNombre = clienteSnapshot.RazonSocialCliente,
                ClienteCiNit = ConstruirCiCompleto(
                    clienteSnapshot.CiCliente,
                    clienteSnapshot.ComplementoCliente
                ),
                UsuarioId = venta.IdUsuario,
                UsuarioNombre = $"Usuario {venta.IdUsuario}",
                FechaVenta = venta.Fecha,
                Total = venta.Total,
                Detalles = detalles.Select(d => new DetalleVentaConfirmadaMessageDto
                {
                    ProductoId = d.IdProducto,
                    ProductoNombre = d.NombreProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            string payload = JsonSerializer.Serialize(evento);

            var outboxMessage = new OutboxMessage(
                messageId: messageId,
                correlationId: venta.CorrelationId,
                exchangeName: _rabbitMqOptions.ExchangeName,
                routingKey: _rabbitMqOptions.RoutingKeys.VentaConfirmada,
                messageType: nameof(VentaConfirmadaMessageDto),
                payload: payload
            );

            int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

            if (filasOutbox <= 0)
                throw new Exception("No se pudo registrar el mensaje Outbox venta.confirmada.");
        }

        private void CrearEventoVentaAnuladaEnOutbox(Venta venta)
        {
            string messageId = Guid.NewGuid().ToString();

            var evento = new VentaAnuladaMessageDto
            {
                MessageId = messageId,
                CorrelationId = venta.CorrelationId,
                VentaId = venta.Id,
                FechaAnulacion = DateTime.Now
            };

            string payload = JsonSerializer.Serialize(evento);

            var outboxMessage = new OutboxMessage(
                messageId: messageId,
                correlationId: venta.CorrelationId,
                exchangeName: _rabbitMqOptions.ExchangeName,
                routingKey: _rabbitMqOptions.RoutingKeys.VentaAnulada,
                messageType: nameof(VentaAnuladaMessageDto),
                payload: payload
            );

            int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

            if (filasOutbox <= 0)
                throw new Exception("No se pudo registrar el mensaje Outbox venta.anulada.");
        }

        private string ConstruirCiCompleto(string ci, string? complemento)
        {
            if (string.IsNullOrWhiteSpace(complemento))
                return ci;

            return $"{ci}-{complemento}";
        }
    }
}