using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using MySqlX.XDevAPI;
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.FactoriaCreadores;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public class MarcaCreadorRepositorio: CreadorRepositorio<Marca>
    {
        public override MarcaRepositorio CrearRepositorio()
        {
            return new MarcaRepositorio();
        }
    }
}
