using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Aplicacion.CasosDeUso
{
    public class CasoDeUsoObtenerProveedor
    {
        private readonly IRepositorio<Proveedor> _repositorioProveedores;

        public CasoDeUsoObtenerProveedor(IRepositorio<Proveedor> repositorioProveedores)
        {
            _repositorioProveedores = repositorioProveedores;
        }

        public async Task<Proveedor> ObtenerPorId(string idProveedor)
        {
            var proveedor = await _repositorioProveedores.ObtenerPorId(idProveedor);
            if (proveedor == null)
            {
                throw new Exception("Proveedor no encontrado");
            }
            return proveedor;
        }

        public async Task<List<Proveedor>> ObtenerTodo()
        {
             return await _repositorioProveedores.ObtenerTodo();
        }
    }
}
