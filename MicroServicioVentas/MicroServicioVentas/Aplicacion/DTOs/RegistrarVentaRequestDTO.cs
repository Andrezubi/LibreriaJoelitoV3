namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class RegistrarVentaRequestDto
    {
        public VentaRequestDto Venta { get; set; } = new();

        public ClienteSnapshotRequestDto Cliente { get; set; } = new();

        public List<DetalleVentaRequestDto> Detalles { get; set; } = new();
    }

    public class VentaRequestDto
    {
        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public string? NombreUsuario { get; set; }
    }

    public class ClienteSnapshotRequestDto
    {
        public int IdCliente { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string? DocumentoCliente { get; set; }

        public string? NitCliente { get; set; }

        public string? TelefonoCliente { get; set; }

        public string? DireccionCliente { get; set; }
    }

    public class DetalleVentaRequestDto
    {
        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombrePresentacion { get; set; } = string.Empty;

        public string? CodigoProducto { get; set; }

        public string? UnidadPresentacion { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
