using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.InterfacesExt
{
    public interface IJwtServicio
    {
        string Generar(int idUsuario, string nombreUsuario, string rol);
        bool Validar(string token);
    }

    public interface IEmailServicio
    {
        Task EnviarCredencialesAsync(string email, string nombreUsuario, string passwordTemporal);
        Task EnviarNotificacionCambioPasswordAsync(string email, string nombreUsuario);
    }

    public interface IUsuarioActual
    {
        string NombreUsuario { get; }
        string Rol { get; }
        string DireccionIp { get; }
    }

}
