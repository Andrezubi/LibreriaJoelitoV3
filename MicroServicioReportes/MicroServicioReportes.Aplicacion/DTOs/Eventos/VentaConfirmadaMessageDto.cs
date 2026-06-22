namespace MicroServicioReportes.Aplicacion.DTOs.Eventos
{
    public class VentaConfirmadaMessageDto
    {
        public string MessageId { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public int VentaId { get; set; }

        public int? ClienteId { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string? ClienteCiNit { get; set; }

        public int? UsuarioId { get; set; }

        public string UsuarioNombre { get; set; } = string.Empty;

        public DateTime FechaVenta { get; set; }

        public decimal Total { get; set; }

        public List<VentaConfirmadaDetalleDto> Detalles { get; set; } = new();
    }
}