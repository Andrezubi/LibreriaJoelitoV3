using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public class OutboxMessageCreadorRepositorio : CreadorRepositorio<OutboxMessage>
    {
        public override OutboxMessageRepositorio CrearRepositorio()
        {
            return new OutboxMessageRepositorio();
        }
    }
}
