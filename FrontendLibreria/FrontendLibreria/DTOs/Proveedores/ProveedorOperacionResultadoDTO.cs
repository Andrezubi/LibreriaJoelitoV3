namespace FrontendLibreria.DTOs.Proveedores
{
    public class ProveedorOperacionResultadoDTO
    {
        public bool Exitoso { get; set; }

        public string? Mensaje { get; set; }

        public Dictionary<string, List<string>> ErroresPorCampo { get; set; } = new();

        public List<string> ErroresGenerales { get; set; } = new();

        public bool TieneErrores =>
            ErroresPorCampo.Any() || ErroresGenerales.Any();
    }
}
