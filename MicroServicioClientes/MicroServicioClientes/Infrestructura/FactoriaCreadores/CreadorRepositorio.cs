using MicroServicioClientes.Aplicacion.Interfaces;

namespace MicroServicioClientes.Infrestructura.FactoriaCreadores 
{
    public abstract class CreadorRepositorio<T>
    {
        public abstract IRepositorio<T> CrearRepositorio();
    }
}
