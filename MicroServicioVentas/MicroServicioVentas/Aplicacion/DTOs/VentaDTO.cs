namespace MicroServicioVentas.Aplicacion.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public int Estado { get; set; }
        public int CiCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
