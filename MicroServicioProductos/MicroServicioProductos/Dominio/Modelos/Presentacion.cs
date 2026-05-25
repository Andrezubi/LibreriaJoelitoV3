namespace MicroServicioProductos.Dominio.Modelos
{
    public class Presentacion
    {
        #region Atributos
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public int? IdUsuario { get; set; }
        #endregion

        #region Constructores
        public Presentacion() { }
        public Presentacion(string nombre, int? idUsuario)
        {
            Nombre = nombre;
            Estado = true;
            FechaRegistro = DateTime.Now;
            FechaUltimaActualizacion = DateTime.Now;
            IdUsuario = idUsuario;
        }
        #endregion
    }
}
