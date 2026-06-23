namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ResumenRecaudacionReporteDto
{
    public string Grupo { get; set; } = string.Empty;
    public int CantidadVentas { get; set; }
    public int CantidadVendida { get; set; }
    public decimal TotalRecaudado { get; set; }
    public decimal Porcentaje { get; set; }
}
