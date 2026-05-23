using MicroServicioUsuarios.Aplicacion.CasosDeUso;
using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MicroServicioUsuarios.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioControlador : ControllerBase
    {
        private readonly ObtenerUsuariosCasoDeUso _obtener;
        private readonly CrearUsuarioCasoDeUso _crear;
        private readonly ActualizarUsuarioCasoDeUso _actualizar;

        public UsuarioControlador(
            ObtenerUsuariosCasoDeUso obtener,
            CrearUsuarioCasoDeUso crear,
            ActualizarUsuarioCasoDeUso actualizar)
        {
            _obtener = obtener;
            _crear = crear;
            _actualizar = actualizar;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// En Servicio_Clientes se leía de Request.Headers["X-IdUsuario"] — cualquiera podía
        /// falsificarlo. Aquí se extrae del JWT firmado por el servidor.
        /// </summary>
        private int GetIdUsuarioDelToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        private string GetIp() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        // ── Endpoints ────────────────────────────────────────────────────────

        /// <summary>Select — nunca incluye el campo password (requisito de rúbrica).</summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodo()
        {
            var resultado = await _obtener.EjecutarAsync();
            return Ok(resultado.Valor);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            if (dto is null) return BadRequest(new { mensaje = "Datos requeridos." });

            var resultado = await _crear.EjecutarAsync(dto, GetIdUsuarioDelToken(), GetIp());

            if (resultado.EsFallido)
                return BadRequest(new { errores = resultado.Error.Mensaje.Split(" | ") });

            return CreatedAtAction(nameof(ObtenerTodo), resultado.Valor);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            if (dto is null) return BadRequest(new { mensaje = "Datos requeridos." });

            var resultado = await _actualizar.EjecutarAsync(
                id, dto, GetIdUsuarioDelToken(), GetIp());

            if (resultado.EsFallido)
            {
                if (resultado.Error.Tipo == TipoError.NoEncontrado)
                    return NotFound(new { mensaje = resultado.Error.Mensaje });

                return BadRequest(new { errores = resultado.Error.Mensaje.Split(" | ") });
            }

            return Ok(resultado.Valor);
        }
    }

}
