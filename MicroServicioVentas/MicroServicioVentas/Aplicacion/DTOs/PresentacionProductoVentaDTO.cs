namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class PresentacionProductoVentaDTO
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public bool EstadoPresentacionProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Presentacion { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}