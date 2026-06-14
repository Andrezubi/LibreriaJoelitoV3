namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class RegistrarVentaRequestDTO
    {
        public VentaRegistroDTO Venta { get; set; } = new VentaRegistroDTO();
        public List<DetalleVentaDTO> Detalles { get; set; } = new List<DetalleVentaDTO>();
    }
}