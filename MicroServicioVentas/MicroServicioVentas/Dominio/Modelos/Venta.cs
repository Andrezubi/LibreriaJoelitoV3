namespace MicroServicioVentas.Dominio.Modelos
{
    public class Venta
    {
        #region Atributos
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        #endregion

        #region Constructores
        public Venta() { }

        public Venta(int idCliente, int idUsuario, DateTime fecha, decimal total, bool estado)
        {
            IdCliente = idCliente;
            IdUsuario = idUsuario;
            Fecha = fecha;
            Total = total;
            Estado = estado;
            FechaRegistro = DateTime.Now;
            FechaUltimaActualizacion = DateTime.Now;
        }
        #endregion
    }
}
