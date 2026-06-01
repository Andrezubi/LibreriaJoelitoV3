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
    public sealed class CrearUsuarioCasoDeUso
    {
        private readonly IUsuarioFabrica _fabrica;
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IBitacoraRepositorio _bitacoraRepo;
        private readonly IEmailServicio _email;

        public CrearUsuarioCasoDeUso(
            IUsuarioFabrica fabrica,
            IUsuarioRepositorio usuarioRepo,
            IEmailServicio email,
            IBitacoraRepositorio bitacoraRepo)
        {
            _fabrica = fabrica;
            _usuarioRepo = usuarioRepo;
            _email = email;
            _bitacoraRepo = bitacoraRepo;
        }

        public async Task<Resultado<UsuarioDto>> EjecutarAsync(
            CrearUsuarioDto dto, int idUsuarioRegistrador)
        {
            // Fábrica valida todos los campos y construye la entidad
            var fabricaResult = await _fabrica.CrearAsync(dto, idUsuarioRegistrador);
            if (fabricaResult.EsFallido)
                return Resultado.Fallido<UsuarioDto>(fabricaResult.Error);

            var (usuario, passwordTemporal) = fabricaResult.Valor;

            await _usuarioRepo.AgregarAsync(usuario);
            await _bitacoraRepo.RegistrarAsync(new Bitacora(
                idUsuarioRegistrador, "INSERT", "Usuario",
                $"Nuevo usuario registrado: {usuario.NombreUsuario}"));
            await _usuarioRepo.GuardarCambiosAsync();

            // Email en fire-and-forget — igual que Servicio_Clientes pero no corta si falla
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
