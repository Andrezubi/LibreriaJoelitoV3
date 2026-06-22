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
    public class FachadaAnularVenta
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public FachadaAnularVenta(IOptions<RabbitMqOptions> rabbitMqOptions, VentaRepositorio ventaRepositorio, DetalleVentaRepositorio detalleVentaRepositorio, OutboxMessageRepositorio outboxMessageRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _outboxMessageRepositorio = outboxMessageRepositorio;
            _rabbitMqOptions = rabbitMqOptions.Value;
        }

        public Result<int> AnularVenta(int idVenta, int idUsuario)
        {
            try
            {
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    var venta = _ventaRepositorio.ObtenerPorId(idVenta);

                    if (venta == null)
                        throw new Exception("No se encontró la venta.");

                    if (venta.Estado == EstadosVenta.Anulada)
                        throw new Exception("La venta ya fue anulada.");

                    if (venta.Estado == EstadosVenta.AnulacionPendiente)
                        throw new Exception("La venta ya tiene una anulación pendiente.");

                    if (venta.Estado != EstadosVenta.Confirmada && venta.Estado != EstadosVenta.StockReservado)
                        throw new Exception("Solo se puede anular una venta confirmada o con stock reservado.");

                    var detalles = _detalleVentaRepositorio.ObtenerPorIdVenta(idVenta);

                    if (!detalles.Any())
                        throw new Exception("La venta no tiene detalles para liberar stock.");

                    _ventaRepositorio.ActualizarEstadoPorId(
                        idVenta, idUsuario,
                        EstadosVenta.AnulacionPendiente,
                        "Anulación iniciada. Pendiente de liberación de stock."
                    );

                    string messageId = Guid.NewGuid().ToString();

                    var liberarStockMessage = new LiberarStockMessageDto
                    {
                        MessageId = messageId,
                        CorrelationId = venta.CorrelationId,
                        IdVenta = venta.Id,
                        IdUsuario = idUsuario,
                        Detalles = detalles.Select(d => new DetalleLiberarStockMessageDto
                        {
                            IdProducto = d.IdProducto,
                            IdPresentacion = d.IdPresentacion,
                            Cantidad = d.Cantidad
                        }).ToList()
                    };

                    string payload = JsonSerializer.Serialize(liberarStockMessage);

                    var outboxMessage = new OutboxMessage(
                        messageId: messageId,
                        correlationId: venta.CorrelationId,
                        exchangeName: _rabbitMqOptions.ExchangeName,
                        routingKey: _rabbitMqOptions.RoutingKeys.StockLiberar,
                        messageType: nameof(LiberarStockMessageDto),
                        payload: payload
                    );

                    int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

                    if (filasOutbox <= 0)
                        throw new Exception("No se pudo registrar el mensaje Outbox para liberar stock.");

                    RepositorioBD.Instancia.Commit();

                    return Result<int>.Success(idVenta);
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result<int>.Failure($"Error al iniciar anulación: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Error inesperado al iniciar anulación: {ex.Message}");
            }
        }
    }
}
