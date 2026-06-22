using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.DTOs.Sagas;
using MicroServicioVentas.Aplicacion.Results;

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

        public Result<ResultadoInicioVentaSagaDto> RegistrarVenta(RegistrarVentaRequestDto request)
        {
            return _realizarVentaServicio.RegistrarVenta(request);
        }

        public Result<int> AnularVenta(int idVenta, int idUsuario)
        {
            return _anularVentaServicio.AnularVenta(idVenta, idUsuario);
        }
    }
}
