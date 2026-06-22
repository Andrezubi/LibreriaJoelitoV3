namespace MicroServicioReportes.Dominio.Entidades;

public class ComprobanteVentaDetalle
{
    public int Id { get; set; }

    public int ComprobanteVentaId { get; set; }
    public ComprobanteVenta ComprobanteVenta { get; set; } = null!;

    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}