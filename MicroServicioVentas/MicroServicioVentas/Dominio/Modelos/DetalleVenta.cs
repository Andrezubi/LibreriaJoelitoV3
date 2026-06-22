namespace MicroServicioVentas.Dominio.Modelos
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombrePresentacion { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }

        public DetalleVenta()
        {
        }
    }
}