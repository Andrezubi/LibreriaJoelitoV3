using FrontendLibreria.DTOs;
using System.Threading.Tasks;

namespace FrontendLibreria.Adaptadores
{
    public interface IUsuarioServicioAdapter
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<(bool Exito, List<string> Errores)> CambiarPasswordAsync(CambiarPasswordDto request);
        Task<(bool Exito, List<string> Errores)> Insertar(SolicitudCrearUsuarioDto request);
        Task<List<UsuarioDto>> ObtenerTodos();
        Task<bool> Eliminar(int id);
        ILogger<UsuarioServicioAdapter> GetLogger();
    }
}
