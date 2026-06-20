namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class Reporte1DTO
    {
        public int Nro { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Presentacion { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendidoBs { get; set; }
        public string EstadoDetalle { get; set; } = string.Empty;
    }
}
