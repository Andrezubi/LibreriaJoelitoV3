namespace MicroServicioProductos.Aplicacion.Eventos
{
    public class EventoStockReservado
    {
        public Guid SagaId { get; set; }


        public Guid VentaId { get; set; }


        public bool Success { get; set; }


        public string Message { get; set; }
    }
}
