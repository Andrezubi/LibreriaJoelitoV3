namespace FrontendLibreria.DTOs.VentaDTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }

        public string CorrelationId { get; set; } = string.Empty;

        public int IdCliente { get; set; }

        public string RazonSocialCliente { get; set; } = string.Empty;

        public string CiCliente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string NombreCliente => RazonSocialCliente;

        public bool EstaPendiente => Estado == "PENDIENTE" || Estado == "ANULACION_PENDIENTE";

        public bool EstaConfirmada => Estado == "CONFIRMADA";

        public bool EstaAnulada => Estado == "ANULADA";

        public bool EstaFallida =>
            Estado == "FALLIDA" ||
            Estado == "STOCK_RECHAZADO";

        public bool PuedeVerDetalle =>
            EstaConfirmada ||
            EstaAnulada;

        public bool PuedeAnular =>
            EstaConfirmada;
    }
}