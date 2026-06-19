using System.Text.Json;
using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class FachadaRealizarVenta
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly OutboxMessageRepositorio _outboxMessageRepositorio;

        public FachadaRealizarVenta()
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
            _detalleVentaRepositorio = new DetalleVentaCreadorRepositorio().CrearRepositorio();
            _outboxMessageRepositorio = new OutboxMessageCreadorRepositorio().CrearRepositorio();
        }

        public Result<ResultadoInicioVentaSagaDto> RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                if (venta == null)
                    return Result<ResultadoInicioVentaSagaDto>.Failure("La venta es obligatoria.");

                if (detalles == null || !detalles.Any())
                    return Result<ResultadoInicioVentaSagaDto>.Failure("La venta debe tener al menos un producto.");

                if (venta.IdCliente <= 0)
                    return Result<ResultadoInicioVentaSagaDto>.Failure("Debe seleccionar un cliente válido.");

                if (venta.IdUsuario <= 0)
                    return Result<ResultadoInicioVentaSagaDto>.Failure("Debe existir un usuario responsable de la venta.");

                foreach (var detalle in detalles)
                {
                    if (detalle.IdProducto <= 0)
                        return Result<ResultadoInicioVentaSagaDto>.Failure("Cada detalle debe tener un producto válido.");

                    if (detalle.IdPresentacion <= 0)
                        return Result<ResultadoInicioVentaSagaDto>.Failure("Cada detalle debe tener una presentación válida.");

                    if (detalle.Cantidad <= 0)
                        return Result<ResultadoInicioVentaSagaDto>.Failure("La cantidad debe ser mayor a cero.");

                    if (detalle.PrecioUnitario < 0)
                        return Result<ResultadoInicioVentaSagaDto>.Failure("El precio unitario no puede ser negativo.");
                }

                if (string.IsNullOrWhiteSpace(venta.CorrelationId))
                    venta.CorrelationId = Guid.NewGuid().ToString();

                venta.Estado = EstadosVenta.Pendiente;
                venta.MotivoFallo = null;
                venta.Total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    int idVenta = _ventaRepositorio.Insertar(venta);

                    if (idVenta <= 0)
                        throw new Exception("No se pudo registrar la cabecera de la venta.");

                    foreach (var detalle in detalles)
                    {
                        detalle.IdVenta = idVenta;
                        detalle.Estado = EstadosDetalleVenta.Pendiente;

                        int filasDetalle = _detalleVentaRepositorio.Insertar(detalle);

                        if (filasDetalle <= 0)
                            throw new Exception($"No se pudo registrar el detalle del producto {detalle.IdProducto}.");
                    }

                    string messageId = Guid.NewGuid().ToString();

                    var validarClienteMessage = new ValidarClienteMessageDto
                    {
                        MessageId = messageId,
                        CorrelationId = venta.CorrelationId,
                        IdVenta = idVenta,
                        IdCliente = venta.IdCliente,
                        IdUsuario = venta.IdUsuario
                    };

                    string payload = JsonSerializer.Serialize(validarClienteMessage);

                    var outboxMessage = new OutboxMessage(
                        correlationId: venta.CorrelationId,
                        routingKey: "cliente.validar",
                        messageType: nameof(ValidarClienteMessageDto),
                        payload: payload
                    );

                    outboxMessage.MessageId = messageId;

                    int filasOutbox = _outboxMessageRepositorio.Insertar(outboxMessage);

                    if (filasOutbox <= 0)
                        throw new Exception("No se pudo registrar el mensaje Outbox para validar cliente.");

                    RepositorioBD.Instancia.Commit();

                    var respuesta = new ResultadoInicioVentaSagaDto
                    {
                        IdVenta = idVenta,
                        CorrelationId = venta.CorrelationId,
                        Estado = EstadosVenta.Pendiente,
                        Mensaje = "Venta registrada como pendiente. La saga fue iniciada correctamente."
                    };

                    return Result<ResultadoInicioVentaSagaDto>.Success(respuesta);
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result<ResultadoInicioVentaSagaDto>.Failure($"Error al iniciar la saga de venta: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Result<ResultadoInicioVentaSagaDto>.Failure($"Error inesperado: {ex.Message}");
            }
        }
    }
}