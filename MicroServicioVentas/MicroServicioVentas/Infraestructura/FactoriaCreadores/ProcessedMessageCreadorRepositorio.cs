using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public class ProcessedMessageCreadorRepositorio : CreadorRepositorio<ProcessedMessage>
    {
        public override ProcessedMessageRepositorio CrearRepositorio()
        {
            return new ProcessedMessageRepositorio();
        }
    }
}