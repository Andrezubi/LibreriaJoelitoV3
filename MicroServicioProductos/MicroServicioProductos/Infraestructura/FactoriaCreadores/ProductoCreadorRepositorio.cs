using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.FactoriaCreadores;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores 
{ 
    public class ProductoCreadorRepositorio:CreadorRepositorio<Producto>
    {
        public override ProductoRepositorio CrearRepositorio()
        {
            return new ProductoRepositorio();
        }
    }
}
