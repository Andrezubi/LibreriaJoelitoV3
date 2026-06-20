namespace MicroServicioVentas.Dominio.Modelos
{
    public class Venta
    {
        public int Id { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? MotivoFallo { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }

        public Venta()
        {
        }
    }
}