

using MicroServicioProductos.Aplicacion.Interfaces;

using MicroServicioProductos.Dominio.Modelos;
using MicroServicios.Infraestructura.FactoriaCreadores;
using MicroServicioProductos.Infrestructura.Persistencia.FactoriaProductos;

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