namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class RegistrarVentaRequestDTO
    {
        public VentaRegistroDTO Venta { get; set; } = new();

        public ClienteVentaSnapshotRequestDTO Cliente { get; set; } = new();

        public List<DetalleVentaDTO> Detalles { get; set; } = new();
    }

    public class ClienteVentaSnapshotRequestDTO
    {
        public int IdCliente { get; set; }

        public string RazonSocial { get; set; } = string.Empty;

        public string Ci { get; set; } = string.Empty;

        public string? Complemento { get; set; }

        public string? Email { get; set; }

        public bool ClienteFrecuente { get; set; }
    }

    public class ResultadoInicioVentaSagaDTO
    {
        public int IdVenta { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}