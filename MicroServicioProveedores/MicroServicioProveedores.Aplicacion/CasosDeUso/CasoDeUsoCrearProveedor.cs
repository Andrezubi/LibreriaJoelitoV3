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
    public class CasoDeUsoCrearProveedor
    {
        private readonly IRepositorio<Proveedor> _repositorioProveedores;
        private readonly ProveedorValidador _proveedorValidador;
        public CasoDeUsoCrearProveedor(IRepositorio<Proveedor> repositorioProveedores, ProveedorValidador proveedorValidador)
        {
            _repositorioProveedores = repositorioProveedores;
            _proveedorValidador = proveedorValidador;
        }

        public async Task<Result> Insertar(RegistrarProveedorDto dto)
        {

            var resultadosValidacion = _proveedorValidador.Validar(dto);

            if (resultadosValidacion.Count > 0)
            {
                return Result.Failure(resultadosValidacion[0].ErrorMessage);

            }

            try
            {
                var nuevoProveedor = new Proveedor(
                    dto.Nombre,
                    dto.Nit,
                    dto.TelefonoContacto,
                    dto.Descripcion,
                    dto.Direccion,
                    dto.IdUsuario
                );

                await _repositorioProveedores.Insertar(nuevoProveedor);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error interno al registrar el proveedor: {ex.Message}");
            }
        }

    }
}
