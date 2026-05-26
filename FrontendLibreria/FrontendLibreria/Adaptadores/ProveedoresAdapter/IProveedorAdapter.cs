using FrontendLibreria.DTOs.Proveedores;

namespace FrontendLibreria.Adaptadores.ProveedoresAdapter
{
    public interface IProveedorAdapter
    {
        Task<List<ProveedorDto>> ObtenerTodosAsync();

        Task<ProveedorDto?> ObtenerPorIdAsync(string id);

        Task<bool> RegistrarAsync(RegistrarProveedorDto proveedor);

        Task<ProveedorOperacionResultadoDTO> RegistrarConResultadoAsync(RegistrarProveedorDto proveedor);

        Task<bool> ActualizarAsync(string id, RegistrarProveedorDto proveedor);

        Task<bool> EliminarAsync(string id, int idUsuario);
    }
}