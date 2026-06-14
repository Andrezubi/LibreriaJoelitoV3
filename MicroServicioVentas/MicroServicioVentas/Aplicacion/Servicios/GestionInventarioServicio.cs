using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class GestionInventarioServicio
    {
        private readonly RealizarVentaServicio _realizarVentaServicio;
        private readonly AnularVentaServicio _anularVentaServicio;

        public GestionInventarioServicio(
            RealizarVentaServicio realizarVentaServicio,
            AnularVentaServicio anularVentaServicio)
        {
            _realizarVentaServicio = realizarVentaServicio;
            _anularVentaServicio = anularVentaServicio;
        }

        public Result<int> RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            return _realizarVentaServicio.RegistrarVenta(venta, detalles);
        }

        public Result<int> AnularVenta(int idVenta, int idEmpleado)
        {
            return _anularVentaServicio.AnularVenta(idVenta, idEmpleado);
        }
    }
}