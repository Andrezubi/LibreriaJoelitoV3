using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public class VentaClienteSnapshotCreadorRepositorio : CreadorRepositorio<VentaClienteSnapshot>
    {
        public override VentaClienteSnapshotRepositorio CrearRepositorio()
        {
            return new VentaClienteSnapshotRepositorio();
        }
    }
}
