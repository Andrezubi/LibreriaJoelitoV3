namespace MicroServicioProductos.Aplicacion.DTOs
{
    public class PresentacionProductoDto
    {
        #region Atributos
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public int IdPresentacion { get; set; }
        public string Presentacion {  get; set; }
        public int FactorConversion { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; } 
        public int? IdUsuario { get; set; }
        #endregion

        
    }
}
