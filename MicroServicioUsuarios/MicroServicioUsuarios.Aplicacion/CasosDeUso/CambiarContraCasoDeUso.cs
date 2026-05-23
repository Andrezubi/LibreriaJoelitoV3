using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Resultados;
using MicroServicioUsuarios.dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.dominio.EntidadesDeValor;

namespace MicroServicioUsuarios.Aplicacion.CasosDeUso
{
    /// <summary>
    /// Extraído de Servicio_Clientes/UsuarioServicio.CambiarContrasena()
    /// + la lógica de confirmación que estaba en AuthController.
    /// Diferencias:
    ///   - Política de contraseña delegada a PasswordPolicy (Value Object).
    ///   - Confirmación de contraseñas validada aquí, no en el controller.
    ///   - Notificación por email al cambio exitoso.
    ///   - Bitácora del evento de cambio.
    /// </summary>
    public sealed class CambiarContraCasoDeUso
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IContraHasher _hasher;
        private readonly IEmailServicio _email;

        public CambiarContraCasoDeUso(
            IUsuarioRepositorio usuarioRepo,
            IContraHasher hasher,
            IEmailServicio email)
        {
            _usuarioRepo = usuarioRepo;
            _hasher = hasher;
            _email = email;
        }

        public async Task<Resultado> EjecutarAsync(
            string nombreUsuario, CambiarPasswordDto dto, string ipOrigen)
        {
            // 1. Confirmación de contraseñas (estaba en AuthController en Servicio_Clientes)
            if (dto.NuevoPassword != dto.ConfirmarPassword)
                return Resultado.Fallido(
                    Error.Validacion("La nueva contraseña y su confirmación no coinciden."));

            // 2. Política — delegada a Value Object (en Servicio_Clientes era método privado)
            var politica = PoliticaContraseña.Validar(dto.NuevoPassword);
            if (politica.EsFallido) return politica;

            // 3. Obtener usuario
            var usuario = await _usuarioRepo.ObtenerPorNombreUsuarioAsync(nombreUsuario);
            if (usuario is null)
                return Resultado.Fallido(Error.NoEncontrado("Usuario no encontrado."));

            // 4. Verificar contraseña actual
            if (!_hasher.Verificar(dto.PasswordActual, usuario.PasswordHash))
                return Resultado.Fallido(
                    Error.Validacion("La contraseña actual es incorrecta."));

            // 5. Actualizar — el método de la entidad limpia MustChangePassword
            var nuevoHash = _hasher.Hashear(dto.NuevoPassword);
            usuario.ActualizarPassword(nuevoHash, usuario.Id);

            await _usuarioRepo.ActualizarAsync(usuario);
            await _usuarioRepo.GuardarCambiosAsync();

            
            // 7. Email de notificación (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try { await _email.EnviarNotificacionCambioPasswordAsync(usuario.Email, nombreUsuario); }
                catch (Exception ex) { Console.WriteLine($"[EMAIL] {ex.Message}"); }
            });

            return Resultado.Exitoso();
        }
    }

}
