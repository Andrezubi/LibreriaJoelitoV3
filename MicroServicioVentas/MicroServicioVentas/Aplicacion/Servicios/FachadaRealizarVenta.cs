using System.Text.Json;
using Microsoft.Extensions.Options;
using MicroServicioVentas.Aplicacion.DTOs;
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
    public class FachadaRealizarVenta
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly VentaClienteSnapshotRepositorio _clienteSnapshotRepositorio;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public FachadaRealizarVenta(IOptions<RabbitMqOptions> rabbitMqOptions)
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
            _detalleVentaRepositorio = new DetalleVentaCreadorRepositorio().CrearRepositorio();
            _clienteSnapshotRepositorio = new VentaClienteSnapshotCreadorRepositorio().CrearRepositorio();
            _outboxMessageRepositorio = new OutboxMessageCreadorRepositorio().CrearRepositorio();
            _rabbitMqOptions = rabbitMqOptions.Value;
        }

        public Result<ResultadoInicioVentaSagaDto> RegistrarVenta(RegistrarVentaRequestDto request)
        {
            try
            {
                var validacion = ValidarSolicitud(request);

                if (validacion.IsFailure)
                    return Result<ResultadoInicioVentaSagaDto>.Failure(validacion.Errors);

                var venta = new Venta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    IdCliente = request.Venta.IdCliente,
                    IdUsuario = request.Venta.IdUsuario,
                    Estado = EstadosVenta.Pendiente,
                    MotivoFallo = null,
                    Total = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
                };

                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    int idVenta = _ventaRepositorio.Insertar(venta);

                    if (idVenta <= 0)
                        throw new Exception("No se pudo registrar la cabecera de la venta.");

                    var clienteSnapshot = new VentaClienteSnapshot
                    {
                        IdVenta = idVenta,
                        IdCliente = request.Cliente.IdCliente,
                        RazonSocialCliente = request.Cliente.RazonSocial.Trim(),
                        CiCliente = request.Cliente.Ci.Trim(),
                        ComplementoCliente = string.IsNullOrWhiteSpace(request.Cliente.Complemento)
                            ? null
                            : request.Cliente.Complemento.Trim(),
                        EmailCliente = string.IsNullOrWhiteSpace(request.Cliente.Email)
                            ? null
                            : request.Cliente.Email.Trim(),
                        ClienteFrecuente = request.Cliente.ClienteFrecuente
                    };

                    int filasClienteSnapshot = _clienteSnapshotRepositorio.Insertar(clienteSnapshot);

                    if (filasClienteSnapshot <= 0)
                        throw new Exception("No se pudo registrar el snapshot del cliente.");

                    foreach (var detalleRequest in request.Detalles)
                    {
                        var detalle = new DetalleVenta
                        {
                            IdVenta = idVenta,
                            IdProducto = detalleRequest.IdProducto,
                            IdPresentacion = detalleRequest.IdPresentacion,
                            NombreProducto = detalleRequest.NombreProducto.Trim(),
                            NombrePresentacion = detalleRequest.NombrePresentacion.Trim(),
                            Cantidad = detalleRequest.Cantidad,
                            PrecioUnitario = detalleRequest.PrecioUnitario,
                            Estado = EstadosDetalleVenta.Pendiente
                        };

                        int filasDetalle = _detalleVentaRepositorio.Insertar(detalle);

                        if (filasDetalle <= 0)
                            throw new Exception($"No se pudo registrar el detalle del producto {detalle.IdProducto}.");
                    }

                    string messageId = Guid.NewGuid().ToString();

                    var reservarStockMessage = new ReservarStockMessageDto
                    {
                        MessageId = messageId,
                        CorrelationId = venta.CorrelationId,
                        IdVenta = idVenta,
                        IdUsuario = venta.IdUsuario,
                        Detalles = request.Detalles.Select(d => new DetalleReservarStockMessageDto
                        {
                            IdProducto = d.IdProducto,
                            IdPresentacion = d.IdPresentacion,
                            Cantidad = d.Cantidad
                        }).ToList()
                    };

                    string payload = JsonSerializer.Serialize(reservarStockMessage);

                    var outboxMessage = new OutboxMessage(
                        messageId: messageId,
                        correlationId: venta.CorrelationId,
                        exchangeName: _rabbitMqOptions.ExchangeName,
                        routingKey: _rabbitMqOptions.RoutingKeys.StockReservar,
                        messageType: nameof(ReservarStockMessageDto),
                        payload: payload
                    );

                    int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

                    if (filasOutbox <= 0)
                        throw new Exception("No se pudo registrar el mensaje Outbox para reservar stock.");

                    RepositorioBD.Instancia.Commit();

                    var respuesta = new ResultadoInicioVentaSagaDto
                    {
                        IdVenta = idVenta,
                        CorrelationId = venta.CorrelationId,
                        Estado = EstadosVenta.Pendiente,
                        Mensaje = "Venta registrada como pendiente. Se guardó el snapshot del cliente y se inició la saga de reserva de stock."
                    };

                    return Result<ResultadoInicioVentaSagaDto>.Success(respuesta);
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result<ResultadoInicioVentaSagaDto>.Failure($"Error al iniciar la venta: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Result<ResultadoInicioVentaSagaDto>.Failure($"Error inesperado: {ex.Message}");
            }
        }

        private Result ValidarSolicitud(RegistrarVentaRequestDto request)
        {
            var errores = new List<string>();

            if (request == null)
                return Result.Failure("La solicitud es obligatoria.");

            if (request.Venta == null)
            {
                errores.Add("La venta es obligatoria.");
            }
            else
            {
                if (request.Venta.IdCliente <= 0)
                    errores.Add("Debe seleccionar un cliente válido.");

                if (request.Venta.IdUsuario <= 0)
                    errores.Add("Debe existir un usuario responsable de la venta.");
            }

            if (request.Cliente == null)
            {
                errores.Add("El snapshot del cliente es obligatorio.");
            }
            else
            {
                if (request.Cliente.IdCliente <= 0)
                    errores.Add("El cliente del snapshot no es válido.");

                if (string.IsNullOrWhiteSpace(request.Cliente.RazonSocial))
                    errores.Add("La razón social del cliente es obligatoria.");

                if (string.IsNullOrWhiteSpace(request.Cliente.Ci))
                    errores.Add("El CI del cliente es obligatorio.");

                if (request.Venta != null && request.Cliente.IdCliente != request.Venta.IdCliente)
                    errores.Add("El IdCliente de la venta y del snapshot no coinciden.");
            }

            if (request.Detalles == null || !request.Detalles.Any())
            {
                errores.Add("La venta debe tener al menos un producto.");
            }
            else
            {
                foreach (var detalle in request.Detalles)
                {
                    if (detalle.IdProducto <= 0)
                        errores.Add("Cada detalle debe tener un producto válido.");

                    if (detalle.IdPresentacion <= 0)
                        errores.Add("Cada detalle debe tener una presentación válida.");

                    if (string.IsNullOrWhiteSpace(detalle.NombreProducto))
                        errores.Add("Cada detalle debe tener el nombre del producto para el comprobante.");

                    if (string.IsNullOrWhiteSpace(detalle.NombrePresentacion))
                        errores.Add("Cada detalle debe tener el nombre de la presentación para el comprobante.");

                    if (detalle.Cantidad <= 0)
                        errores.Add("La cantidad debe ser mayor a cero.");

                    if (detalle.PrecioUnitario < 0)
                        errores.Add("El precio unitario no puede ser negativo.");
                }
            }

            return errores.Any()
                ? Result.Failure(errores)
                : Result.Success();
        }
    }
}