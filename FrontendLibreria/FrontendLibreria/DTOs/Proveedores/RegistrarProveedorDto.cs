namespace FrontendLibreria.DTOs.Proveedores
{
    public class RegistrarProveedorDto
    {
        public string? Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Nit { get; set; }

        public int TelefonoContacto { get; set; }

        public string? Descripcion { get; set; }

        public string Direccion { get; set; } = string.Empty;

        public int IdUsuario { get; set; } = 1;
    }
}
