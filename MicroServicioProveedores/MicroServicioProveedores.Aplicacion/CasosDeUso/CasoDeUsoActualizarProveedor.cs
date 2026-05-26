using MicroServicioProveedores.Aplicacion.DTOs;
using MicroServicioProveedores.Aplicacion.Results;
using MicroServicioProveedores.Aplicacion.Validadores;
using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Aplicacion.CasosDeUso
{
    public class CasoDeUsoActualizarProveedor
    {
        private readonly IRepositorio<Proveedor> _repositorioProveedores;
        private readonly ProveedorValidador _proveedorValidador;

        public CasoDeUsoActualizarProveedor(IRepositorio<Proveedor> repositorioProveedores, ProveedorValidador proveedorValidador)
        {
            _repositorioProveedores = repositorioProveedores;
            _proveedorValidador = proveedorValidador;
        }

        public async Task<Result> Actualizar(RegistrarProveedorDto proveedorDTO)
        {
            var resultadosValidacion = _proveedorValidador.Validar(proveedorDTO);

            if (resultadosValidacion.Count > 0)
            {
                throw new Exception(resultadosValidacion[0].ErrorMessage);
            }

            var proveedorExistente = await _repositorioProveedores.ObtenerPorId(proveedorDTO.Id);

            if (proveedorExistente == null)
            {
                throw new Exception("Proveedor no encontrado");
            }

            proveedorExistente.Nombre = proveedorDTO.Nombre;
            proveedorExistente.Nit = proveedorDTO.Nit;
            proveedorExistente.TelefonoContacto = proveedorDTO.TelefonoContacto;
            proveedorExistente.Descripcion = proveedorDTO.Descripcion;
            proveedorExistente.Direccion = proveedorDTO.Direccion;

            var resultado = await _repositorioProveedores.Actualizar(proveedorExistente);

            if (!resultado)
            {
                return Result.Failure("Error al actualizar el proveedor");
            }

            return Result.Success();
        }
    }
}
