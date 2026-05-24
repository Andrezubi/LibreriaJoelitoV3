using MicroServicioProveedores.Aplicacion.CasosDeUso;
using MicroServicioProveedores.Aplicacion.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly CasoDeUsoObtenerProveedor _casoDeUsoObtenerProveedor;
        private readonly CasoDeUsoCrearProveedor _casoDeUsoCrearProveedor;
        private readonly CasoDeUsoActualizarProveedor _casoDeUsoActualizarProveedor;
        private readonly CasoDeUsoEliminarProveedor _casoDeUsoEliminarProveedor;

        public ProveedorController(
            CasoDeUsoObtenerProveedor casoDeUsoObtenerProveedor,
            CasoDeUsoCrearProveedor casoDeUsoCrearProveedor,
            CasoDeUsoActualizarProveedor casoDeUsoActualizarProveedor,
            CasoDeUsoEliminarProveedor casoDeUsoEliminarProveedor)
        {
            _casoDeUsoObtenerProveedor = casoDeUsoObtenerProveedor;
            _casoDeUsoCrearProveedor = casoDeUsoCrearProveedor;
            _casoDeUsoActualizarProveedor = casoDeUsoActualizarProveedor;
            _casoDeUsoEliminarProveedor = casoDeUsoEliminarProveedor;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var proveedores = await _casoDeUsoObtenerProveedor.ObtenerTodo();
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var proveedor = await _casoDeUsoObtenerProveedor.ObtenerPorId(id);
                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarProveedorDto dto)
        {
            var resultado = await _casoDeUsoCrearProveedor.Insertar(dto);

            if (resultado.IsSuccess)
            {
                return Ok(new { mensaje = "Proveedor registrado exitosamente." });
            }

            return BadRequest(new { error = resultado.Errors });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] RegistrarProveedorDto dto)
        {
            try
            {
                dto.Id = id;

                var resultado = await _casoDeUsoActualizarProveedor.Actualizar(dto);

                if (resultado.IsSuccess)
                {
                    return Ok(new { mensaje = "Proveedor actualizado exitosamente." });
                }

                return BadRequest(new { error = resultado.Errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            try
            {
                var resultado = await _casoDeUsoEliminarProveedor.Eliminar(id);

                if (resultado)
                {
                    return Ok(new { mensaje = "Proveedor eliminado exitosamente." });
                }

                return BadRequest(new { error = "Error al eliminar el proveedor." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}