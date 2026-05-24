using MicroServicioUsuarios.Aplicacion.CasosDeUso;
using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MicroServicioUsuarios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ObtenerUsuariosCasoDeUso _obtenerUseCase;
        private readonly CrearUsuarioCasoDeUso _crearUseCase;
        private readonly ActualizarUsuarioCasoDeUso _actualizarUseCase;
        private readonly CambiarContraCasoDeUso _cambiarPasswordUseCase;

        public UsuariosController(
            ObtenerUsuariosCasoDeUso obtenerUseCase,
            CrearUsuarioCasoDeUso crearUseCase,
            ActualizarUsuarioCasoDeUso actualizarUseCase,
            CambiarContraCasoDeUso cambiarPasswordUseCase)
        {
            _obtenerUseCase = obtenerUseCase;
            _crearUseCase = crearUseCase;
            _actualizarUseCase = actualizarUseCase;
            _cambiarPasswordUseCase = cambiarPasswordUseCase;
        }

        /// <summary>
        /// Select — retorna todos los usuarios sin exponer PasswordHash.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var resultado = await _obtenerUseCase.EjecutarAsync();
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(resultado.Valor);
        }

        /// <summary>
        /// Insert — crea un usuario con generación automática de nombre de usuario
        /// y contraseña temporal enviada por email.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            var idRegistrador = ObtenerIdUsuarioDelToken();
            var ip = ObtenerIp();

            var resultado = await _crearUseCase.EjecutarAsync(dto, idRegistrador, ip);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return CreatedAtAction(nameof(ObtenerTodos), new { id = resultado.Valor.Id }, resultado.Valor);
        }

        /// <summary>
        /// Update — actualiza datos personales del usuario (sin cambiar password desde aquí).
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            var idModificador = ObtenerIdUsuarioDelToken();
            var ip = ObtenerIp();

            var resultado = await _actualizarUseCase.EjecutarAsync(id, dto, idModificador, ip);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(resultado.Valor);
        }

        /// <summary>
        /// Cambio de contraseña — protegido con [Authorize].
        /// Requiere contraseña actual + nueva contraseña (2 veces).
        /// Aplica políticas de seguridad y registra en bitácora.
        /// </summary>
        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var nombreUsuario = ObtenerNombreUsuarioDelToken();
            var ip = ObtenerIp();

            var resultado = await _cambiarPasswordUseCase.EjecutarAsync(nombreUsuario, dto, ip);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente. Inicie sesión nuevamente." });
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private int ObtenerIdUsuarioDelToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        private string ObtenerNombreUsuarioDelToken()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("unique_name")?.Value
                ?? string.Empty;
        }

        private string ObtenerIp() =>
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        private IActionResult MapearError(Error error) => error.Tipo switch
        {
            TipoError.Validacion => BadRequest(new { error = error.Mensaje }),
            TipoError.NoEncontrado => NotFound(new { error = error.Mensaje }),
            TipoError.NoAutorizado => Unauthorized(new { error = error.Mensaje }),
            TipoError.Conflicto => Conflict(new { error = error.Mensaje }),
            _ => StatusCode(500, new { error = error.Mensaje })
        };
    }
}
