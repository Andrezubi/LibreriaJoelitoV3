namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class DetalleLiberarStockMessageDto
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public int Cantidad { get; set; }
    }
}
