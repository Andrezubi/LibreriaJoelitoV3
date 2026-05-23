using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.dominio.Resultados;
using MicroServicioUsuarios.dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.CasosDeUso
{
    /// <summary>
    /// Extraído de Servicio_Clientes/UsuarioServicio.InsertarAsync().
    /// Diferencias:
    ///   - Validaciones delegadas a Value Objects (no a ValidadorEmpleado estático).
    ///   - Creación delegada a IUsuarioFabrica (Factory Method).
    ///   - IdUsuario extraído del JWT, no hardcodeado a 1.
    ///   - Bitácora registrada aquí, no en el Controller.
    ///   - Email en fire-and-forget con Task.Run para no bloquear la respuesta.
    /// </summary>
    public sealed class CrearUsuarioCasoDeUso
    {
        private readonly IUsuarioFabrica _fabrica;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IEmailServicio _email;

        public CrearUsuarioCasoDeUso(
            IUsuarioFabrica fabrica,
            IUsuarioRepositorio usuarioRepo,
            IEmailServicio email)
        {
            _fabrica = fabrica;
            _usuarioRepo = usuarioRepo;
            _email = email;
        }

        public async Task<Resultado<UsuarioDto>> EjecutarAsync(
            CrearUsuarioDto dto, int idUsuarioRegistrador, string ipOrigen)
        {
            // 1. Fábrica valida todos los campos y construye la entidad
            var fabricaResult = await _fabrica.CrearAsync(dto, idUsuarioRegistrador);
            if (fabricaResult.EsFallido)
                return Resultado.Fallido<UsuarioDto>(fabricaResult.Error);

            var (usuario, passwordTemporal) = fabricaResult.Valor;

            // 2. Persistir
            await _usuarioRepo.AgregarAsync(usuario);
            await _usuarioRepo.GuardarCambiosAsync();

       
            // 4. Email en fire-and-forget — igual que Servicio_Clientes pero no corta si falla
            _ = Task.Run(async () =>
            {
                try
                {
                    await _email.EnviarCredencialesAsync(
                        usuario.Email, usuario.NombreUsuario, passwordTemporal);
                }
                catch (Exception ex)
                {
                    // En producción: reemplazar con ILogger
                    Console.WriteLine($"[EMAIL] Error al enviar credenciales: {ex.Message}");
                }
            });

            return Resultado.Exitoso(MapearDto(usuario));
        }

        private static UsuarioDto MapearDto(Usuario u) => new(
            u.Id, u.NombreUsuario, u.NombreCompleto, u.CiCompleto,
            u.Email, u.Telefono, u.DireccionDomicilio, u.Rol,
            u.Estado, u.MustChangePassword,
            u.FechaNacimiento, u.FechaIngreso, u.FechaRegistro);
    }

}
