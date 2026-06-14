namespace MicroServicioVentas.Dominio.Modelos
{
    public class DetalleVenta
    {
        #region Atributos
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
        #endregion

        #region Constructores
        public DetalleVenta() { }
        public DetalleVenta(int idVenta, int idProducto, int idPresentacion, int cantidad, decimal precioUnitario)
        {
            IdVenta = idVenta;
            IdProducto = idProducto;
            IdPresentacion = idPresentacion;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }   
        #endregion
    }

}
