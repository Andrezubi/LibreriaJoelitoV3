namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ClienteReporteDto
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string CiNit { get; set; } = string.Empty;
}
