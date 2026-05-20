namespace MicroServicioClientes.Dominio.Modelos
{
    public class Cliente
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string? Email { get; set; }
        public bool ClienteFrecuente { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public int? IdUsuario { get; set; }

        public Cliente() { }

    }
}
