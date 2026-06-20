namespace MicroServicioVentas.Dominio.Modelos
{
    public class VentaClienteSnapshot
    {
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public int IdCliente { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string? DocumentoCliente { get; set; }

        public string? NitCliente { get; set; }

        public string? TelefonoCliente { get; set; }

        public string? DireccionCliente { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
