namespace MicroServicioVentas.Dominio.Modelos
{
    public class VentaClienteSnapshot
    {
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public int IdCliente { get; set; }

        public string RazonSocialCliente { get; set; } = string.Empty;

        public string CiCliente { get; set; } = string.Empty;

        public string? ComplementoCliente { get; set; }

        public string? EmailCliente { get; set; }

        public bool ClienteFrecuente { get; set; }

        public DateTime FechaRegistro { get; set; }

        public VentaClienteSnapshot()
        {
        }

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