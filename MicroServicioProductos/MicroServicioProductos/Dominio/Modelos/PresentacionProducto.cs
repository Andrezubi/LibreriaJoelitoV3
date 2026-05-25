namespace MicroServicioProductos.Dominio.Modelos
{
    public class PresentacionProducto
    {
        #region Atributos
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public int FactorConversion { get; set; }
        public decimal Precio { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; } 
        public int? IdUsuario { get; set; }
        #endregion

        #region Constructores
        public PresentacionProducto() { }
        public PresentacionProducto(int idProducto, int idPresentacion, int factorConversion, decimal precio, int? idUsuario)
        {
            IdProducto = idProducto;
            IdPresentacion = idPresentacion;
            FactorConversion = factorConversion;
            Precio = precio;
            Estado = true;
            FechaRegistro = DateTime.Now;
            FechaUltimaActualizacion = DateTime.Now;
            IdUsuario = idUsuario;
        }
        #endregion
    }
}
