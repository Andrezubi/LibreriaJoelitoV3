using System;

namespace MicroServicioUsuarios.dominio.Entidades
{
    public class Bitacora
    {
        public int Id { get; private set; }
        public int IdUsuario { get; private set; }
        public string Accion { get; private set; } = string.Empty;
        public string Tabla { get; private set; } = string.Empty;
        public DateTime Fecha { get; private set; }
        public string Descripcion { get; private set; } = string.Empty;

        private Bitacora() { }

        public Bitacora(int idUsuario, string accion, string tabla, string descripcion)
        {
            IdUsuario = idUsuario;
            Accion = accion;
            Tabla = tabla;
            Fecha = DateTime.UtcNow;
            Descripcion = descripcion;
        }
    }
}
