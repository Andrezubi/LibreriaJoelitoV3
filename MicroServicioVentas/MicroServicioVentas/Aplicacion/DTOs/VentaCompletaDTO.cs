namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaCompletaDTO
    {
        public VentaCabeceraDTO Venta { get; set; } = new();
        public List<DetalleVentaExtraDTO> Detalles { get; set; } = new();
    }
}
