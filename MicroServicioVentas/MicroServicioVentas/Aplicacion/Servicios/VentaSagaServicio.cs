using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class VentaSagaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly ProcessedMessageRepositorio _processedMessageRepositorio;

        public VentaSagaServicio()
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
            _detalleVentaRepositorio = new DetalleVentaCreadorRepositorio().CrearRepositorio();
            _processedMessageRepositorio = new ProcessedMessageCreadorRepositorio().CrearRepositorio();
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

                    if (venta.Estado == EstadosVenta.Pendiente || venta.Estado == EstadosVenta.StockReservado)
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

                    if (venta.Estado == EstadosVenta.Pendiente || venta.Estado == EstadosVenta.StockReservado)
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
    }
}
