namespace MicroServicioProductos.Aplicacion.Comandos
{
    public class ComandoLiberarStock
    {

        public string CorrelationId { get; set; } = string.Empty;

        public int IdVenta { get; set; }

        public int ProductoId { get; set; }

        public int Cantidad { get; set; }

    }
}
