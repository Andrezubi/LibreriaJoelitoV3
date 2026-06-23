namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class PresentacionProductoVentaDTO
    {
        public int IdProducto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public int IdPresentacion { get; set; }

        public string Presentacion { get; set; } = string.Empty;

        public int FactorConversion { get; set; }

        public decimal Precio { get; set; }

        public decimal PrecioUnitario { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string? Marca { get; set; }

        public bool Estado { get; set; }

        public bool EstadoPresentacionProducto { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }

        public int? IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioFinal
        {
            get
            {
                return Precio > 0
                    ? Precio
                    : PrecioUnitario;
            }
        }
    }
}