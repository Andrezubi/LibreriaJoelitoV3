using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public class OutboxMessageCreadorRepositorio : CreadorRepositorio<OutboxMessage>
    {
        public override OutboxMessageRepositorio CrearRepositorio()
        {
            return new OutboxMessageRepositorio();
        }
    }
}