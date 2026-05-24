using FrontendLibreria.DTOs;
using System.Threading.Tasks;

namespace FrontendLibreria.Adaptadores
{
    public interface IUsuarioServicioAdapter
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> CambiarPasswordAsync(CambiarPasswordDto request);
    }
}
