
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.FactoriaCreadores;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public class PresentacionProductoCreadorRepositorio:CreadorRepositorio<PresentacionProducto>
    {
        public override PresentacionProductoRepositorio CrearRepositorio()
        {
            return new PresentacionProductoRepositorio();
        }
    }
}
