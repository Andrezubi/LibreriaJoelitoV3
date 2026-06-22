using MicroServicioReportes.Aplicacion.DTOs.Eventos;
using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Servicios
{
    public class ComprobanteVentaSagaServicio : IComprobanteVentaSagaServicio
    {
        private const string RoutingKeyVentaConfirmada = "venta.confirmada";
        private const string RoutingKeyVentaAnulada = "venta.anulada";

        private readonly IComprobanteVentaRepositorio _comprobanteVentaRepositorio;
        private readonly IProcessedMessageRepositorio _processedMessageRepositorio;
        private readonly IUnidadTrabajo _unidadTrabajo;

        public ComprobanteVentaSagaServicio(
            IComprobanteVentaRepositorio comprobanteVentaRepositorio,
            IProcessedMessageRepositorio processedMessageRepositorio,
            IUnidadTrabajo unidadTrabajo)
        {
            _comprobanteVentaRepositorio = comprobanteVentaRepositorio;
            _processedMessageRepositorio = processedMessageRepositorio;
            _unidadTrabajo = unidadTrabajo;
        }

        public void ProcesarVentaConfirmada(VentaConfirmadaMessageDto evento)
        {
            ValidarVentaConfirmada(evento);

            if (_processedMessageRepositorio.ExisteMessageId(evento.MessageId))
                return;

            _unidadTrabajo.BeginTransaction();

            try
            {
                if (_processedMessageRepositorio.ExisteMessageId(evento.MessageId))
                {
                    _unidadTrabajo.Commit();
                    return;
                }

                if (!_comprobanteVentaRepositorio.ExistePorVentaId(evento.VentaId))
                {
                    var comprobante = CrearComprobanteDesdeEvento(evento);

                    int comprobanteVentaId =
                        _comprobanteVentaRepositorio.RegistrarComprobante(comprobante);

                    _comprobanteVentaRepositorio.RegistrarDetalles(
                        comprobanteVentaId,
                        comprobante.Detalles
                    );
                }

                RegistrarMensajeProcesado(
                    evento.MessageId,
                    evento.CorrelationId,
                    RoutingKeyVentaConfirmada
                );

                _unidadTrabajo.Commit();
            }
            catch
            {
                _unidadTrabajo.Rollback();
                throw;
            }
        }

        public void ProcesarVentaAnulada(VentaAnuladaMessageDto evento)
        {
            ValidarVentaAnulada(evento);

            if (_processedMessageRepositorio.ExisteMessageId(evento.MessageId))
                return;

            _unidadTrabajo.BeginTransaction();

            try
            {
                if (_processedMessageRepositorio.ExisteMessageId(evento.MessageId))
                {
                    _unidadTrabajo.Commit();
                    return;
                }

                if (!_comprobanteVentaRepositorio.ExistePorVentaId(evento.VentaId))
                {
                    throw new InvalidOperationException(
                        $"No existe comprobante para la venta {evento.VentaId}. No se puede anular."
                    );
                }

                _comprobanteVentaRepositorio.MarcarComoAnulado(
                    evento.VentaId,
                    evento.FechaAnulacion
                );

                RegistrarMensajeProcesado(
                    evento.MessageId,
                    evento.CorrelationId,
                    RoutingKeyVentaAnulada
                );

                _unidadTrabajo.Commit();
            }
            catch
            {
                _unidadTrabajo.Rollback();
                throw;
            }
        }

        private static ComprobanteVenta CrearComprobanteDesdeEvento(
            VentaConfirmadaMessageDto evento)
        {
            var comprobante = new ComprobanteVenta
            {
                VentaId = evento.VentaId,
                CorrelationId = evento.CorrelationId,
                MessageId = evento.MessageId,

                NumeroComprobante = GenerarNumeroComprobante(
                    evento.VentaId,
                    evento.FechaVenta
                ),

                ClienteId = evento.ClienteId,
                ClienteNombre = evento.ClienteNombre,
                ClienteCiNit = evento.ClienteCiNit,

                UsuarioId = evento.UsuarioId,
                UsuarioNombre = evento.UsuarioNombre,

                FechaVenta = evento.FechaVenta,
                FechaGeneracion = DateTime.Now,

                Total = evento.Total,

                Estado = "GENERADO",
                FechaAnulacion = null,

                CreadoEn = DateTime.Now,
                ActualizadoEn = null
            };

            comprobante.Detalles = evento.Detalles
                .Select(detalle => new ComprobanteVentaDetalle
                {
                    ProductoId = detalle.ProductoId,
                    ProductoNombre = detalle.ProductoNombre,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                })
                .ToList();

            return comprobante;
        }

        private static string GenerarNumeroComprobante(
            int ventaId,
            DateTime fechaVenta)
        {
            return $"COMP-{fechaVenta.Year}-{ventaId:D6}";
        }

        private void RegistrarMensajeProcesado(
            string messageId,
            string correlationId,
            string routingKey)
        {
            var processedMessage = new ProcessedMessage
            {
                MessageId = messageId,
                CorrelationId = correlationId,
                RoutingKey = routingKey,
                ProcessedAt = DateTime.Now
            };

            _processedMessageRepositorio.RegistrarMensajeProcesado(processedMessage);
        }

        private static void ValidarVentaConfirmada(VentaConfirmadaMessageDto evento)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            if (string.IsNullOrWhiteSpace(evento.MessageId))
                throw new ArgumentException("El MessageId es obligatorio.");

            if (string.IsNullOrWhiteSpace(evento.CorrelationId))
                throw new ArgumentException("El CorrelationId es obligatorio.");

            if (evento.VentaId <= 0)
                throw new ArgumentException("El VentaId no es válido.");

            if (string.IsNullOrWhiteSpace(evento.ClienteNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(evento.UsuarioNombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (evento.Total < 0)
                throw new ArgumentException("El total de la venta no puede ser negativo.");

            if (evento.Detalles == null || !evento.Detalles.Any())
                throw new ArgumentException("La venta debe tener al menos un detalle.");
        }

        private static void ValidarVentaAnulada(VentaAnuladaMessageDto evento)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            if (string.IsNullOrWhiteSpace(evento.MessageId))
                throw new ArgumentException("El MessageId es obligatorio.");

            if (string.IsNullOrWhiteSpace(evento.CorrelationId))
                throw new ArgumentException("El CorrelationId es obligatorio.");

            if (evento.VentaId <= 0)
                throw new ArgumentException("El VentaId no es válido.");

            if (evento.FechaAnulacion == default)
                throw new ArgumentException("La fecha de anulación no es válida.");
        }
    }
}