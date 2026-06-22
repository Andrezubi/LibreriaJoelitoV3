namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class BitacoraReporteDto
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Tabla { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
