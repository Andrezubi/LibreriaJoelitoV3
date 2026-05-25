namespace MicroServicioClientes.Aplicacion.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string? Email { get; set; }
        public bool ClienteFrecuente { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public int? IdUsuario { get; set; }

        // Helper de solo lectura para mostrar el CI completo en la vista
        public string CiCompleto => string.IsNullOrEmpty(Complemento)
            ? Ci
            : $"{Ci}-{Complemento}";
    }
}