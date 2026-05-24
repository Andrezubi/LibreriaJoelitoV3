using System;

namespace MicroServicioUsuarios.dominio.Entidades
{
    public class Bitacora
    {
        public int Id { get; private set; }
        public string Usuario { get; private set; } = string.Empty;
        public string Accion { get; private set; } = string.Empty;
        public string Modulo { get; private set; } = string.Empty;
        public string Ip { get; private set; } = string.Empty;
        public string Detalle { get; private set; } = string.Empty;
        public DateTime Fecha { get; private set; }

        private Bitacora() { }

        public Bitacora(string usuario, string accion, string modulo, string ip, string detalleNuevo)
        {
            Usuario = usuario;
            Accion = accion;
            Modulo = modulo;
            Ip = ip;
            Detalle = detalleNuevo;
            Fecha = DateTime.UtcNow;
        }
    }
}
