using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroServicioProveedores.Dominio.Interfaces;

namespace MicroservicioProveedores.Infraestructura.Persistence.FactoriaCreadores
{
    public abstract class CreadorRepositorio<T>
    {
        public abstract IRepositorio<T> CrearRepositorio();
    }
}
