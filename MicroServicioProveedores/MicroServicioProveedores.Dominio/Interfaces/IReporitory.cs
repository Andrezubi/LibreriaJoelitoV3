using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Dominio.Interfaces
{
    public interface IRepositorio<T>
    {
        Task Insertar(T t);
        Task<bool> Actualizar(T t);
        Task<bool> Eliminar(T t);
        Task<List<T>> ObtenerTodo();
        Task<T> ObtenerPorId(string id);
    }
}
