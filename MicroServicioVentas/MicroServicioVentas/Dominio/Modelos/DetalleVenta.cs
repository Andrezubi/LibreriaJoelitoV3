using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Dominio.Modelos
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int IdPresentacion { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombrePresentacion { get; set; } = string.Empty;

        public string? CodigoProducto { get; set; }

        public string? UnidadPresentacion { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public string Estado { get; set; } = EstadosDetalleVenta.Pendiente;

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }

        public DetalleVenta()
        {
            Estado = EstadosDetalleVenta.Pendiente;
        }

        public DetalleVenta(
            int idVenta,
            int idProducto,
            int idPresentacion,
            string nombreProducto,
            string nombrePresentacion,
            int cantidad,
            decimal precioUnitario)
        {
            IdVenta = idVenta;
            IdProducto = idProducto;
            IdPresentacion = idPresentacion;
            NombreProducto = nombreProducto;
            NombrePresentacion = nombrePresentacion;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Estado = EstadosDetalleVenta.Pendiente;
        }

        public void MarcarReservado()
        {
            Estado = EstadosDetalleVenta.Reservado;
        }

        public void MarcarConfirmado()
        {
            Estado = EstadosDetalleVenta.Confirmado;
        }

        public void MarcarLiberado()
        {
            Estado = EstadosDetalleVenta.Liberado;
        }

        public void MarcarFallido()
        {
            Estado = EstadosDetalleVenta.Fallido;
        }
    }
}
