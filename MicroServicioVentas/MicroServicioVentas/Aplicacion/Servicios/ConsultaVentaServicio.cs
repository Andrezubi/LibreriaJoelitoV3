using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class ConsultaVentaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;

        public ConsultaVentaServicio()
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
            _detalleVentaRepositorio = new DetalleVentaCreadorRepositorio().CrearRepositorio();
        }

        public List<Venta> CargarVentas()
        {
            return _ventaRepositorio.ObtenerTodo();
        }

        public List<PresentacionProductoVentaDTO> getPresentacionProductosByFrase(string frase)
        {
            // Temporal:
            // Productos ya no pertenece al MicroServicioVentas.
            // Esto luego debe consultarse desde MicroServicioProductos.
            return new List<PresentacionProductoVentaDTO>();
        }

        public Result<PresentacionProductoVentaDTO> GetPresentacionProductoByIds(int idProducto, int idPresentacion)
        {
            return Result<PresentacionProductoVentaDTO>.Failure(
                "Consulta de presentación/producto deshabilitada temporalmente. Debe resolverse desde MicroServicioProductos."
            );
        }

        public Result<byte[]> GenerarComprobantePdf(int idVenta)
        {
            return Result<byte[]>.Failure(
                "Generación de PDF deshabilitada temporalmente mientras se refactoriza la consulta de ventas."
            );
        }

        public Result<VentaCompletaDTO> ObtenerVentaCompleta(int idVenta)
        {
            return Result<VentaCompletaDTO>.Failure(
                "Consulta de venta completa deshabilitada temporalmente mientras se refactoriza la lectura de ventas."
            );
        }

        public List<Reporte1DTO> ObtenerReporteServicios()
        {
            return new List<Reporte1DTO>();
        }
    }
}