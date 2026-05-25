namespace MicroServicioClientes.Dominio.Modelos
{
    public class Bitacora
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Accion { get; set; }
        public string Tabla { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }

        public Bitacora() { }

        public Bitacora(int idUsuario, string accion, string tabla, DateTime fecha, string descripcion)
        {
            IdUsuario = idUsuario;
            Accion = accion;
            Tabla = tabla;
            Fecha = fecha;
            Descripcion = descripcion;
        }
    }
}
