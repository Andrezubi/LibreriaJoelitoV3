namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaCabeceraDTO
    {
        public int Id { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public string EstadoVenta { get; set; } = string.Empty;

        public int IdCliente { get; set; }

        public string RazonSocialCliente { get; set; } = string.Empty;

        public string CiCliente { get; set; } = string.Empty;

        public string? ComplementoCliente { get; set; }

        public string? EmailCliente { get; set; }

        public bool ClienteFrecuente { get; set; }

        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string CiCompleto
        {
            get
            {
                return string.IsNullOrWhiteSpace(ComplementoCliente)
                    ? CiCliente
                    : $"{CiCliente}-{ComplementoCliente}";
            }
        }
    }
}