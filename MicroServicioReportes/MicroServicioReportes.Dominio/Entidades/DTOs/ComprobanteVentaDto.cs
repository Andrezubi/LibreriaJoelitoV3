namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ComprobanteVentaDto
{
    public int IdVenta { get; set; }
    public DateTime FechaVenta { get; set; }
    public string Estado { get; set; } = "Confirmada";
    public ClienteReporteDto Cliente { get; set; } = new();
    public List<DetalleVentaReporteDto> Detalles { get; set; } = new();
    public decimal Total => Detalles.Sum(d => d.Importe);
    public string TotalLiteral { get; set; } = string.Empty;
    public string UsuarioGenerador { get; set; } = string.Empty;
}
