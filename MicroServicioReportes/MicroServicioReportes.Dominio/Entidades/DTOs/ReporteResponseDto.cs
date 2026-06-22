namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class ReporteResponseDto
{
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string NombreArchivo { get; set; } = "reporte";
}
