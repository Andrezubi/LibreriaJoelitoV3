using MicroServicioVentas.Dominio.Modelos;

namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class RegistrarVentaRequestDto
    {
        public Venta Venta { get; set; }
        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
