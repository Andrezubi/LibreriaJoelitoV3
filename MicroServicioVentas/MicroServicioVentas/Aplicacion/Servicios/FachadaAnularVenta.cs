using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos.Enum;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class FachadaAnularVenta
    {
        private readonly VentaRepositorio _ventaRepositorio;

        public FachadaAnularVenta()
        {
            _ventaRepositorio = new VentaCreadorRepositorio().CrearRepositorio();
        }

        public Result<int> AnularVenta(int idVenta, int idEmpleado)
        {
            try
            {
                var venta = _ventaRepositorio.ObtenerPorId(idVenta);

                if (venta == null)
                    return Result<int>.Failure("No se encontró la venta.");

                if (venta.Estado == EstadosVenta.Anulada)
                    return Result<int>.Failure("La venta ya fue anulada.");

                if (venta.Estado == EstadosVenta.AnulacionPendiente)
                    return Result<int>.Failure("La venta ya tiene una anulación pendiente.");

                int filas = _ventaRepositorio.ActualizarEstadoPorId(
                    idVenta,
                    EstadosVenta.AnulacionPendiente,
                    "Anulación iniciada. Pendiente de compensación por saga."
                );

                if (filas <= 0)
                    return Result<int>.Failure("No se pudo iniciar la anulación de la venta.");

                return Result<int>.Success(idVenta);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Error inesperado al iniciar anulación: {ex.Message}");
            }
        }
    }
}