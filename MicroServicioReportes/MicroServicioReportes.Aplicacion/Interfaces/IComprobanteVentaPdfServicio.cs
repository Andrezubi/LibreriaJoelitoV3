using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Interfaces
{
    public interface IComprobanteVentaPdfServicio
    {
        byte[] GenerarComprobanteVenta(ComprobanteVenta comprobante);
    }
}