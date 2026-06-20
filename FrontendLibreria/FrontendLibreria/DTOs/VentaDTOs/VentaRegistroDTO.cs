namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class VentaRegistroDTO
    {
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public bool Estado { get; set; }
    }
}