using MicroServicioClientes.Infrestructura.Persistencia.FactoriaProductos;
using MySqlX.XDevAPI;
using MicroServicioClientes.Aplicacion.Interfaces;
using MicroServicioClientes.Dominio.Modelos;

namespace MicroServicioClientes.Infrestructura.FactoriaCreadores
{
    public class ClienteCreadorRepositorio : CreadorRepositorio<Cliente>
    {
        public override ClienteRepositorio CrearRepositorio()
        {
            return new ClienteRepositorio();
        }
    }
}
