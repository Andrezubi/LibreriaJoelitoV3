using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public class ProcessedMessageCreadorRepositorio:CreadorRepositorio<ProcessedMessage>
    {
        public override ProcessedMessageRepositorio CrearRepositorio()
        {
            return new ProcessedMessageRepositorio();
        }
       
    }
}
