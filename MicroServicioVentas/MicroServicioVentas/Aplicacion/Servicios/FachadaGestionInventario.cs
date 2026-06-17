using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class FachadaGestionInventario
    {
        private readonly FachadaRealizarVenta _realizarVentaServicio;
        private readonly FachadaAnularVenta _anularVentaServicio;

        public FachadaGestionInventario(
            FachadaRealizarVenta realizarVentaServicio,
            FachadaAnularVenta anularVentaServicio)
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