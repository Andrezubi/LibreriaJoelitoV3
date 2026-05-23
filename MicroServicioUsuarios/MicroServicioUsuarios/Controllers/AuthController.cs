using MicroServicioUsuarios.Aplicacion.CasosDeUso;
using MicroServicioUsuarios.Aplicacion.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MicroServicioUsuarios.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthControlador : ControllerBase
    {
        private readonly InicioSesionUsuarioCasoDeUso _login;
        private readonly CambiarContraCasoDeUso _cambiarPassword;

        public AuthControlador(
            InicioSesionUsuarioCasoDeUso login,
            CambiarContraCasoDeUso cambiarPassword)
        {
            _login = login;
            _cambiarPassword = cambiarPassword;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.NombreUsuario)
                            || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { mensaje = "Usuario y contraseña son requeridos." });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
            var resultado = await _login.EjecutarAsync(dto, ip);

            if (resultado.EsFallido)
                return Unauthorized(new { mensaje = resultado.Error.Mensaje });

            // Cookie HttpOnly — igual que Servicio_Clientes
            Response.Cookies.Append("AuthToken", resultado.Valor.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return Ok(resultado.Valor);
        }

        [HttpPost("cambiar-contrasena")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            if (dto is null) return BadRequest(new { mensaje = "Datos requeridos." });

            // IdUsuario extraído del JWT — NO del header como en Servicio_Clientes
            var nombreUsuario = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(nombreUsuario))
                return Unauthorized(new { mensaje = "Sesión inválida." });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
            var resultado = await _cambiarPassword.EjecutarAsync(nombreUsuario, dto, ip);

            if (resultado.EsFallido)
                return BadRequest(new { errores = resultado.Error.Mensaje.Split(" | ") });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }
}
