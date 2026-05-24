using MicroServicioUsuarios.Aplicacion.CasosDeUso;
using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Resultados;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioUsuarios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly InicioSesionUsuarioCasoDeUso _loginUseCase;

        public AuthController(InicioSesionUsuarioCasoDeUso loginUseCase)
        {
            _loginUseCase = loginUseCase;
        }

        /// <summary>
        /// Endpoint de login — genera JWT firmado con id, nombre_usuario y rol.
        /// Retorna MustChangePassword para que el frontend fuerce el cambio en primer inicio.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
            var resultado = await _loginUseCase.EjecutarAsync(dto, ip);

            if (resultado.EsFallido)
                return MapearError(resultado.Error);

            return Ok(resultado.Valor);
        }

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
