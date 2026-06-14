using System.Data;

namespace MicroServicioVentas.Aplicacion.Interfaces
{
    public interface IRepositorio<T>
    {
        int Insertar(T t);
        int Actualizar(T t);
        int Eliminar(T t);
        List<T> ObtenerTodo();
    }
}
