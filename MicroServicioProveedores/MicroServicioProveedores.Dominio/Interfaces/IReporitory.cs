using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Dominio.Interfaces
{
    internal interface IReporitory<T>
    {
        int Insertar(T t);
        int Actualizar(T t);
        int Eliminar(T t);
        List<T> ObtenerTodo();
    }
}
