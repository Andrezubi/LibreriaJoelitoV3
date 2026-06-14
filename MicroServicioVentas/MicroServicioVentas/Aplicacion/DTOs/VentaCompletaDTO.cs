namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaCompletaDTO
    {
        public VentaCabeceraDTO Venta { get; set; } = new VentaCabeceraDTO();
        public List<DetalleVentaExtraDTO> Detalles { get; set; } = new List<DetalleVentaExtraDTO>();
    }
}
