using MicroServicioReportes.Aplicacion.DTOs.Eventos;

namespace MicroServicioReportes.Aplicacion.Interfaces
{
    public interface IComprobanteVentaSagaServicio
    {
        void ProcesarVentaConfirmada(VentaConfirmadaMessageDto evento);

        void ProcesarVentaAnulada(VentaAnuladaMessageDto evento);
    }
}