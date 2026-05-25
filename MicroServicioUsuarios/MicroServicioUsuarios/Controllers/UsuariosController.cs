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
        private readonly EliminarUsuarioCasoDeUso _eliminarUseCase;

        public UsuariosController(
            ObtenerUsuariosCasoDeUso obtenerUseCase,
            CrearUsuarioCasoDeUso crearUseCase,
            ActualizarUsuarioCasoDeUso actualizarUseCase,
            CambiarContraCasoDeUso cambiarPasswordUseCase,
            EliminarUsuarioCasoDeUso eliminarUseCase)
        {
            _obtenerUseCase = obtenerUseCase;
            _crearUseCase = crearUseCase;
            _actualizarUseCase = actualizarUseCase;
            _cambiarPasswordUseCase = cambiarPasswordUseCase;
            _eliminarUseCase = eliminarUseCase;
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
            if (idRegistrador is null)
                return TokenSinIdentificador();

            var resultado = await _crearUseCase.EjecutarAsync(dto, idRegistrador.Value);
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
            if (idModificador is null)
                return TokenSinIdentificador();

            var resultado = await _actualizarUseCase.EjecutarAsync(id, dto, idModificador.Value);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(resultado.Valor);
        }

        /// <summary>
        /// Baja Lógica — desactiva un usuario sin eliminarlo de la BD.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var idModificador = ObtenerIdUsuarioDelToken();
            if (idModificador is null)
                return TokenSinIdentificador();

            var resultado = await _eliminarUseCase.EjecutarAsync(id, idModificador.Value);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(new { mensaje = "Usuario eliminado exitosamente." });
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
            var idUsuario = ObtenerIdUsuarioDelToken();
            if (idUsuario is null)
                return TokenSinIdentificador();

            var nombreUsuario = ObtenerNombreUsuarioDelToken();

            var resultado = await _cambiarPasswordUseCase.EjecutarAsync(idUsuario.Value, nombreUsuario, dto);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente. Inicie sesión nuevamente." });
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private int? ObtenerIdUsuarioDelToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");
            return claim is not null && int.TryParse(claim.Value, out var id) && id > 0 ? id : null;
        }

        private string ObtenerNombreUsuarioDelToken()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("unique_name")?.Value
                ?? string.Empty;
        }

        private IActionResult TokenSinIdentificador() =>
            Unauthorized(new { error = "El token no contiene un identificador de usuario válido." });

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
