using MicroservicioProveedores.Infraestructura.ProductosConcretos;
using MicroServicioProveedores.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicioProveedores.Infraestructura.Persistence.FactoriaCreadores
{
    public class CreadorProveedorRepositorio: CreadorRepositorio<Proveedor>
    {
        public override ProveedorRepositorio CrearRepositorio()
        {
            return new ProveedorRepositorio();
        }
    }
}
