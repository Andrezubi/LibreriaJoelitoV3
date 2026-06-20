using MicroServicioVentas.Aplicacion.DTOs;

namespace MicroServicioVentas.Aplicacion.Interfaces
{
    public interface IPdfServicio
    {
        byte[] GenerarComprobanteVenta(VentaCompletaDTO ventaCompleta);
    }
}
