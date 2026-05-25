

using MicroServicioProductos.Aplicacion.Interfaces;

using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public class PresentacionCreadorRepositorio : CreadorRepositorio<Presentacion>
    {
        public override PresentacionRepositorio CrearRepositorio()
        {
            return new PresentacionRepositorio();
        }
    }
}