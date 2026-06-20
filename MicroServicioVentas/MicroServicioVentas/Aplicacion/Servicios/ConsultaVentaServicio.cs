using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class ConsultaVentaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly VentaClienteSnapshotRepositorio _clienteSnapshotRepositorio;
        private readonly IPdfServicio _pdfServicio;

        public ConsultaVentaServicio(IPdfServicio pdfServicio, VentaRepositorio ventaRepositorio, DetalleVentaRepositorio detalleVentaRepositorio, VentaClienteSnapshotRepositorio clienteSnapshotRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _clienteSnapshotRepositorio = clienteSnapshotRepositorio;
            _pdfServicio = pdfServicio;
        }

        public List<VentaDTO> CargarVentas()
        {
            return _ventaRepositorio.ObtenerResumenVentas();
        }

        public Result<VentaCompletaDTO> ObtenerVentaCompleta(int idVenta)
        {
            try
            {
                var venta = _ventaRepositorio.ObtenerPorId(idVenta);

                if (venta == null)
                    return Result<VentaCompletaDTO>.Failure("No se encontró la venta.");

                var clienteSnapshot = _clienteSnapshotRepositorio.ObtenerPorIdVenta(idVenta);

                if (clienteSnapshot == null)
                    return Result<VentaCompletaDTO>.Failure("No se encontró el snapshot del cliente para esta venta.");

                var detalles = _detalleVentaRepositorio.ObtenerDetalleExtraPorIdVenta(idVenta);

                var ventaCompleta = new VentaCompletaDTO
                {
                    Venta = new VentaCabeceraDTO
                    {
                        Id = venta.Id,
                        CorrelationId = venta.CorrelationId,
                        EstadoVenta = venta.Estado,
                        IdCliente = venta.IdCliente,
                        RazonSocialCliente = clienteSnapshot.RazonSocialCliente,
                        CiCliente = clienteSnapshot.CiCliente,
                        ComplementoCliente = clienteSnapshot.ComplementoCliente,
                        EmailCliente = clienteSnapshot.EmailCliente,
                        ClienteFrecuente = clienteSnapshot.ClienteFrecuente,
                        IdUsuario = venta.IdUsuario,
                        Fecha = venta.Fecha,
                        Total = venta.Total
                    },
                    Detalles = detalles
                };

                return Result<VentaCompletaDTO>.Success(ventaCompleta);
            }
            catch (Exception ex)
            {
                return Result<VentaCompletaDTO>.Failure($"Error al obtener venta completa: {ex.Message}");
            }
        }

        public Result<byte[]> GenerarComprobantePdf(int idVenta)
        {
            try
            {
                var resultadoVenta = ObtenerVentaCompleta(idVenta);

                if (!resultadoVenta.IsSuccess)
                    return Result<byte[]>.Failure(resultadoVenta.Errors);

                byte[] pdf = _pdfServicio.GenerarComprobanteVenta(resultadoVenta.Value);

                if (pdf.Length == 0)
                    return Result<byte[]>.Failure("No se pudo generar el comprobante de venta.");

                return Result<byte[]>.Success(pdf);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"Error al generar comprobante PDF: {ex.Message}");
            }
        }

        public List<Reporte1DTO> ObtenerReporteServicios()
        {
            return _detalleVentaRepositorio.ObtenerReporteServicios();
        }
    }
}