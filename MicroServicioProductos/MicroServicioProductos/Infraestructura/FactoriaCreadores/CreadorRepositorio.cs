using MicroServicioProductos.Aplicacion.Interfaces;

namespace MicroServicioProductos.Infraestructura.FactoriaCreadores
{
    public abstract class CreadorRepositorio<T>
    {
        public abstract IRepositorio<T> CrearRepositorio();
    }
}
