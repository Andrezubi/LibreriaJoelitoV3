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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var resultado = await _loginUseCase.EjecutarAsync(dto);

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
