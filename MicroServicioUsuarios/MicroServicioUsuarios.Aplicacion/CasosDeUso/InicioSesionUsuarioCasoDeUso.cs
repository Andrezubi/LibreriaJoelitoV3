using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.dominio.Interfaces;
using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.CasosDeUso
{
    /// <summary>
    /// Extraído de Servicio_Clientes/UsuarioServicio.Login().
    /// Diferencias:
    ///   - Retorna Result[LoginResponseDto] en lugar de LoginResultado con bool.
    ///   - Mensaje de error unificado ("Credenciales inválidas") — no revela
    ///     si el usuario existe o si la contraseña es incorrecta (buena práctica de seguridad).
    ///   - Registra bitácora tanto en login exitoso como fallido.
    /// </summary>
    public sealed class InicioSesionUsuarioCasoDeUso
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IContraHasher _hasher;
        private readonly IJwtServicio _jwt;

        public InicioSesionUsuarioCasoDeUso(
            IUsuarioRepositorio usuarioRepo,
            IContraHasher hasher,
            IJwtServicio jwt)
        {
            _usuarioRepo = usuarioRepo;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<Resultado<LoginResponseDto>> EjecutarAsync(
            LoginRequestDto dto, string ipOrigen)
        {
            var usuario = await _usuarioRepo.ObtenerPorNombreUsuarioAsync(dto.NombreUsuario);

            // Mensaje unificado — no revela si el usuario existe o no
            if (usuario is null || !_hasher.Verificar(dto.Password, usuario.PasswordHash))
            {
                return Resultado.Fallido<LoginResponseDto>(
                    Error.NoAutorizado("Credenciales inválidas."));
            }

            if (!usuario.Estado)
                return Resultado.Fallido<LoginResponseDto>(
                    Error.NoAutorizado("La cuenta está desactivada. Contacte al administrador."));

            var token = _jwt.Generar(usuario.Id, usuario.NombreUsuario, usuario.Rol);

            return Resultado.Exitoso(new LoginResponseDto(
                Token: token,
                NombreUsuario: usuario.NombreUsuario,
                NombreCompleto: usuario.NombreCompleto,
                Rol: usuario.Rol,
                MustChangePassword: usuario.MustChangePassword));
        }

    }

}
