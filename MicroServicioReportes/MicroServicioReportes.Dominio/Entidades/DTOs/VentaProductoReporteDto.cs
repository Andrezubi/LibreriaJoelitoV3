namespace MicroServicioReportes.Dominio.Entidades.DTOs;

public class VentaProductoReporteDto
{
    public int NumeroVenta { get; set; }
    public DateTime FechaVenta { get; set; }
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Importe => CantidadVendida * PrecioUnitario;
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string EstadoVenta { get; set; } = "Confirmada";
}
