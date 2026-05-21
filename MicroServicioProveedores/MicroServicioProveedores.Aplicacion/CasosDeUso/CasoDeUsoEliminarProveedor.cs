using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Aplicacion.CasosDeUso
{
    public class CasoDeUsoEliminarProveedor
    {
        private readonly IRepositorio<Proveedor> _repositorioProveedores;

        public CasoDeUsoEliminarProveedor(IRepositorio<Proveedor> repositorioProveedores)
        {
            _repositorioProveedores = repositorioProveedores;
        }  

        public async Task Eliminar(string idProveedor)
        {
            var proveedor = await _repositorioProveedores.ObtenerPorId(idProveedor);
            if (proveedor == null)
            {
                throw new Exception("Proveedor no encontrado");
            }
            _repositorioProveedores.Eliminar(proveedor);
        }
    }
}
