using Microsoft.AspNetCore.Mvc;
using MicroServicioProductos.Aplicacion.Servicios;
using MicroServicioProductos.Dominio.Modelos;

namespace MicroServicioProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarcaController : ControllerBase
    {
        private readonly MarcaServicio _marcaServicio;

        public MarcaController(MarcaServicio marcaServicio)
        {
            _marcaServicio = marcaServicio;
        }

        [HttpGet]
        public IActionResult ObtenerTodo() => Ok(_marcaServicio.ObtenerTodo());

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(int id)
        {
            var marca = _marcaServicio.ObtenerPorId(id);
            if (marca.Id == 0) return NotFound();
            return Ok(marca);
        }

        [HttpPost]
        public IActionResult Insertar([FromBody] Marca marca)
        {
            var result = _marcaServicio.Insertar(marca);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });
            return Ok(new { success = true });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] Marca marca)
        {
            marca.Id = id;
            var result = _marcaServicio.Actualizar(marca);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id, [FromQuery] int idUsuario)
        {
            var marca = new Marca
            {
                Id = id,
                IdUsuario = idUsuario
            };
            var filas = _marcaServicio.Eliminar(marca);

            if (filas == 0)
                return BadRequest("No se eliminó ningún registro");

            return Ok();
        }
    }
}
