using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Dominio.Modelos
{
    public class Venta
    {
        #region Atributos
        public int Id { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = EstadosVenta.Pendiente;

        public string? MotivoFallo { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }
        #endregion

        #region Constructores
        public Venta()
        {
            CorrelationId = Guid.NewGuid().ToString();
            Estado = EstadosVenta.Pendiente;
        }

        public Venta(int idCliente, int idUsuario, decimal total)
        {
            CorrelationId = Guid.NewGuid().ToString();
            IdCliente = idCliente;
            IdUsuario = idUsuario;
            Total = total;
            Estado = EstadosVenta.Pendiente;
        }
        #endregion

        #region Metodos
        public void MarcarClienteValidado()
        {
            Estado = EstadosVenta.ClienteValidado;
            MotivoFallo = null;
        }

        public void MarcarClienteRechazado(string motivo)
        {
            Estado = EstadosVenta.ClienteRechazado;
            MotivoFallo = motivo;
        }

        public void MarcarStockReservado()
        {
            Estado = EstadosVenta.StockReservado;
            MotivoFallo = null;
        }

        public void MarcarStockRechazado(string motivo)
        {
            Estado = EstadosVenta.StockRechazado;
            MotivoFallo = motivo;
        }

        public void Confirmar()
        {
            Estado = EstadosVenta.Confirmada;
            MotivoFallo = null;
        }

        public void Fallar(string motivo)
        {
            Estado = EstadosVenta.Fallida;
            MotivoFallo = motivo;
        }

        public void IniciarAnulacion()
        {
            Estado = EstadosVenta.AnulacionPendiente;
        }

        public void Anular()
        {
            Estado = EstadosVenta.Anulada;
            MotivoFallo = null;
        }
        #endregion
    }
}