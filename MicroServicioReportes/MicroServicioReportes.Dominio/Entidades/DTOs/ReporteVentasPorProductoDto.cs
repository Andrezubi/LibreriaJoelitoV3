namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ReporteVentasPorProductoDto
{
    public DateTime FechaGeneracion { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public ReporteRequestDto Filtros { get; set; } = new();
    public IReadOnlyCollection<string> MicroserviciosConsultados { get; set; } =
        Array.Empty<string>();
    public IReadOnlyCollection<VentaProductoReporteDto> Ventas { get; set; } =
        Array.Empty<VentaProductoReporteDto>();
    public int TotalUnidadesVendidas { get; set; }
    public decimal TotalRecaudado { get; set; }
}
