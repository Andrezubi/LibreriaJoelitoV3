namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class ValidarClienteMessageDto
    {
        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public int IdVenta { get; set; }

        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }
    }
}