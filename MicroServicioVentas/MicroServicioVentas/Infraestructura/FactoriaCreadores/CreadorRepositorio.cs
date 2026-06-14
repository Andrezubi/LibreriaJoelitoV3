using MicroServicioVentas.Aplicacion.Interfaces;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public abstract class CreadorRepositorio<T>
    {
        public abstract IRepositorio<T> CrearRepositorio();
    }
}
