namespace MicroServicioVentas.Aplicacion.DTOs.Sagas
{
    public class ResultadoInicioVentaSagaDto
    {
        public int IdVenta { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}