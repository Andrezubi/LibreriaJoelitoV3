namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class RegistrarVentaRequestDto
    {
        public VentaRegistroDto Venta { get; set; } = new();

        public ClienteVentaSnapshotRequestDto Cliente { get; set; } = new();

        public List<DetalleVentaRequestDto> Detalles { get; set; } = new();
    }

    public class VentaRegistroDto
    {
        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }
    }

    public class ClienteVentaSnapshotRequestDto
    {
        public int IdCliente { get; set; }

        public string RazonSocial { get; set; } = string.Empty;

        public string Ci { get; set; } = string.Empty;

        public string? Complemento { get; set; }

        public string? Email { get; set; }

        public bool ClienteFrecuente { get; set; }
    }

    public class DetalleVentaRequestDto
    {
        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombrePresentacion { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}