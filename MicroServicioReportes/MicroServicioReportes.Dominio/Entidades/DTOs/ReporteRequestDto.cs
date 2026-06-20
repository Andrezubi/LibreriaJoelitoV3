namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ReporteRequestDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? IdProducto { get; set; }
    public int? IdCliente { get; set; }
    public string Usuario { get; set; } = string.Empty;
}
