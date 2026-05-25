using Microsoft.AspNetCore.Mvc;
using MicroServicioClientes.Aplicacion.Servicios;
using MicroServicioClientes.Dominio.Modelos;

namespace MicroServicioClientes.Aplicacion.Controllers
{
    //[Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly Infrestructura.Persistencia.FactoriaProductos.BitacoraRepositorio _bitacoraRepo;

        public ClienteController(ClienteServicio clienteServicio, Infrestructura.Persistencia.FactoriaProductos.BitacoraRepositorio bitacoraRepo)
        {
            _clienteServicio = clienteServicio;
            _bitacoraRepo = bitacoraRepo;
        }

        private int GetIdUsuarioFromHeader()
        {
            if (Request.Headers.TryGetValue("X-IdUsuario", out var idStr))
            {
                if (int.TryParse(idStr, out int id)) return id;
            }
            return 0;
        }

        [HttpGet]
        public IActionResult ObtenerTodo() => Ok(_clienteServicio.ObtenerTodo());

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(int id)
        {
            var cliente = _clienteServicio.ObtenerPorId(id);
            if (cliente.Id == 0) return NotFound();
            return Ok(cliente);
        }

        [HttpGet("buscar-ci/{ci}")]
        public IActionResult ObtenerPorCi(string ci)
        {
            var cliente = _clienteServicio.ObtenerPorCi(ci);
            if (cliente == null) return NotFound();
            return Ok(cliente);
        }

        [HttpGet("similares-ci/{ci}")]
        public IActionResult ObtenerSimilarCi(string ci)
            => Ok(_clienteServicio.ObtenerSimilarCi(ci));

        [HttpPost]
        public IActionResult Insertar([FromBody] Cliente cliente)
        {
            var result = _clienteServicio.Insertar(cliente);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });

            // AUDITORÍA
           _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "INSERT", "Cliente", $"Nuevo cliente registrado con ID: {result.Value}");
            return Ok(new { success = true, id = result.Value });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] Cliente cliente)
        {
            cliente.Id = id;
            var result = _clienteServicio.Actualizar(cliente);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });

            // AUDITORÍA
           _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "UPDATE", "Cliente", $"Cliente actualizado ID: {id}");
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id, [FromBody] Cliente cliente)
        {
            cliente.Id = id;
            _clienteServicio.Eliminar(cliente);

            // AUDITORÍA
            _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "DELETE", "Cliente", $"Cliente eliminado ID: {id}");
            return Ok();
        }
    }
}