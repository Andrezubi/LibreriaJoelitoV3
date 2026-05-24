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
        /// Baja Lógica — desactiva un usuario sin eliminarlo de la BD.
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var idModificador = ObtenerIdUsuarioDelToken();

            var resultado = await _eliminarUseCase.EjecutarAsync(id, idModificador);
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
            var nombreUsuario = ObtenerNombreUsuarioDelToken();
            var ip = ObtenerIp();

            var resultado = await _cambiarPasswordUseCase.EjecutarAsync(nombreUsuario, dto, ip);
            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente. Inicie sesión nuevamente." });
        }

        // ── ENDPOINT TEMPORAL PARA PRUEBAS (SEMILLA) ────────────────────────
        [AllowAnonymous]
        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin([FromServices] MicroServicioUsuarios.dominio.Interfaces.IContraHasher hasher, [FromServices] MicroServicioUsuarios.dominio.Interfaces.IUsuarioRepositorio repo)
        {
            var existe = await repo.ExisteNombreUsuarioAsync("admin.prueba");
            if (existe) return Ok("El admin de prueba ya existe. Su password es temporal123");

            var hash = hasher.Hashear("temporal123");

            var admin = new MicroServicioUsuarios.dominio.Entidades.Usuario(
                "admin.prueba",
                hash,
                "Administrador",
                MicroServicioUsuarios.dominio.EntidadesDeValor.NombrePersona.Crear("Admin").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.NombrePersona.Crear("Prueba").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.NombrePersona.Crear("Sistema").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.CarnetIdentidad.Crear("12345678").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.FechaNacimiento.Crear(new DateOnly(1990, 1, 1)).Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.Email.Crear("admin@libreria.com").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.Direccion.Crear("Calle Falsa 123").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.Telefono.Crear("77777777").Valor!,
                MicroServicioUsuarios.dominio.EntidadesDeValor.FechaIngreso.Crear(DateOnly.FromDateTime(DateTime.Now)).Valor!,
                0
            );

            await repo.AgregarAsync(admin);
            await repo.GuardarCambiosAsync();

            return Ok(new { mensaje = "Usuario creado exitosamente para pruebas.", usuario = "admin.prueba", password = "temporal123" });
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
