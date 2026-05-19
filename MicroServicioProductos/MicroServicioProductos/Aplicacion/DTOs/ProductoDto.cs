namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public int IdMarca { get; set; }
        public string? NombreMarca { get; set; }
        public int Stock { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int IdUsuario { get; set; }
    }
}
