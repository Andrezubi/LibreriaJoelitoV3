namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class SolicitudAgregarPresentacion
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public int FactorConversion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int IdUsuario { get; set; }
    }
}
