namespace MicroServicioReportes.Dominio.Entidades;

public class ComprobanteVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;

    public string NumeroComprobante { get; set; } = string.Empty;

    public int? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteCiNit { get; set; }

    public int? UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;

    public DateTime FechaVenta { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;

    public decimal Total { get; set; }

    public string Estado { get; set; } = "GENERADO";
    public DateTime? FechaAnulacion { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.Now;
    public DateTime? ActualizadoEn { get; set; }

    public List<ComprobanteVentaDetalle> Detalles { get; set; } = new();
}