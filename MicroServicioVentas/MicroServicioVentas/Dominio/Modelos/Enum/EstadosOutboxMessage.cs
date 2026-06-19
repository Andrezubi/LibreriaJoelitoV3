namespace MicroServicioVentas.Dominio.Modelos.Enum
{
    public static class EstadosOutboxMessage
    {
        public const string Pending = "PENDING";
        public const string Published = "PUBLISHED";
        public const string Failed = "FAILED";
    }
}