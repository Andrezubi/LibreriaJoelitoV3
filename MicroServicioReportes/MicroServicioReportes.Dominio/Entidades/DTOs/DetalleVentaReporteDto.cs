namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class DetalleVentaReporteDto
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Importe => Cantidad * PrecioUnitario;
}
