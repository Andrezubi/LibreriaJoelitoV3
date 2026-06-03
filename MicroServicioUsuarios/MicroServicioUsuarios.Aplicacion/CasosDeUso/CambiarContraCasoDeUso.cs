using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Resultados;
using MicroServicioUsuarios.dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.dominio.EntidadesDeValor;
using MicroServicioUsuarios.dominio.Entidades;
namespace MicroServicioUsuarios.Aplicacion.CasosDeUso
{
    public sealed class CambiarContraCasoDeUso
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IContraHasher _hasher;
        private readonly IEmailServicio _email;
        private readonly IBitacoraRepositorio _bitacoraRepo;

        public CambiarContraCasoDeUso(
            IUsuarioRepositorio usuarioRepo,
            IContraHasher hasher,
            IEmailServicio email,
            IBitacoraRepositorio bitacoraRepo)
        {
            _usuarioRepo = usuarioRepo;
            _hasher = hasher;
            _email = email;
            _bitacoraRepo = bitacoraRepo;
        }

        public async Task<Resultado> EjecutarAsync(
            int idUsuario, string nombreUsuario, CambiarPasswordDto dto)
        {
            // Confirmación de contraseñas (estaba en AuthController en Servicio_Clientes)
            if (dto.NuevoPassword != dto.ConfirmarPassword)
                return Resultado.Fallido(
                    Error.Validacion("La nueva contraseña y su confirmación no coinciden."));

            // Política — delegada a Value Object (en Servicio_Clientes era método privado)
            var politica = PoliticaContraseña.Validar(dto.NuevoPassword);
            if (politica.EsFallido) return politica;

            // Obtener usuario
            var usuario = await _usuarioRepo.ObtenerPorNombreUsuarioAsync(nombreUsuario);
            if (usuario is null)
                return Resultado.Fallido(Error.NoEncontrado("Usuario no encontrado."));

            // Verificar contraseña actual
            if (!_hasher.Verificar(dto.PasswordActual, usuario.PasswordHash))
                return Resultado.Fallido(
                    Error.Validacion("La contraseña actual es incorrecta."));

            // Actualizar — el método de la entidad limpia MustChangePassword
            var nuevoHash = _hasher.Hashear(dto.NuevoPassword);
            usuario.ActualizarPassword(nuevoHash, idUsuario);

            await _usuarioRepo.ActualizarAsync(usuario);
            await _bitacoraRepo.RegistrarAsync(new Bitacora(
                idUsuario, "UPDATE", "Usuario", "Contraseña del usuario actualizada"));
            await _usuarioRepo.GuardarCambiosAsync();

            // Email de notificación (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try { await _email.EnviarNotificacionCambioPasswordAsync(usuario.Email, nombreUsuario); }
                catch (Exception ex) { Console.WriteLine($"[EMAIL] {ex.Message}"); }
            });

            return Resultado.Exitoso();
        }
    }

}
