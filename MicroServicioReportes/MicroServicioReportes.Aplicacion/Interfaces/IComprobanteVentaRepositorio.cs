using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Interfaces
{
    public interface IComprobanteVentaRepositorio
    {
        bool ExistePorVentaId(int ventaId);

        int RegistrarComprobante(ComprobanteVenta comprobante);

        void RegistrarDetalles(
            int comprobanteVentaId,
            IEnumerable<ComprobanteVentaDetalle> detalles
        );

        ComprobanteVenta? ObtenerPorVentaId(int ventaId);

        void MarcarComoAnulado(
            int ventaId,
            DateTime fechaAnulacion
        );
    }
}