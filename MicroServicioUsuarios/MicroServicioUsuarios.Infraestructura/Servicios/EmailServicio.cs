using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.Servicios
{
    public sealed class EmailServicio : IEmailServicio
    {
        private readonly EmailSettings _cfg;

        public EmailServicio(IOptions<EmailSettings> opciones)
            => _cfg = opciones.Value;

        public async Task EnviarCredencialesAsync(
            string email, string nombreUsuario, string passwordTemporal)
        {
            string asunto = "Bienvenido al sistema — Sus credenciales de acceso";
            string cuerpo = $@"
            <div style='font-family:Arial,sans-serif;max-width:500px'>
              <h3 style='color:#1a237e'>Bienvenido/a al sistema</h3>
              <p>Su cuenta ha sido creada exitosamente.</p>
              <table style='border-collapse:collapse;width:100%'>
                <tr>
                  <td style='padding:8px;background:#f5f5f5;font-weight:bold'>Usuario:</td>
                  <td style='padding:8px'>{nombreUsuario}</td>
                </tr>
                <tr>
                  <td style='padding:8px;background:#f5f5f5;font-weight:bold'>Contraseña temporal:</td>
                  <td style='padding:8px'><code>{passwordTemporal}</code></td>
                </tr>
              </table>
              <p style='color:#d32f2f;margin-top:16px'>
                <strong>Importante:</strong> Al iniciar sesión por primera vez,
                el sistema le pedirá cambiar esta contraseña.
              </p>
            </div>";

            await EnviarAsync(email, asunto, cuerpo);
        }

        public async Task EnviarNotificacionCambioPasswordAsync(
            string email, string nombreUsuario)
        {
            string asunto = "Contraseña actualizada correctamente";
            string cuerpo = $@"
            <div style='font-family:Arial,sans-serif;max-width:500px'>
              <h3 style='color:#1a237e'>Contraseña actualizada</h3>
              <p>Hola <strong>{nombreUsuario}</strong>,</p>
              <p>Su contraseña fue actualizada el {DateTime.Now:dd/MM/yyyy HH:mm}.</p>
              <p>Si usted no realizó este cambio, contacte al administrador de inmediato.</p>
            </div>";

            await EnviarAsync(email, asunto, cuerpo);
        }

        // ── Helper privado — misma lógica que Servicio_Clientes ──────────────
        private async Task EnviarAsync(string para, string asunto, string cuerpo)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_cfg.RemitAlias, _cfg.Usuario));
            mensaje.To.Add(MailboxAddress.Parse(para));
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = cuerpo }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.Servidor, _cfg.Puerto,
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_cfg.Usuario, _cfg.Password);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }
    }

}
