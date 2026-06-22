namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class DetalleLiberarStockMessageDto
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public int Cantidad { get; set; }
    }
}
