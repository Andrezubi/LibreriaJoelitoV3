namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class DetalleVentaExtraDTO
    {
        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Presentacion { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}