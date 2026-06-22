using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Http;
namespace FrontendLibreria.Adaptadores.Cliente
{
    public interface IAdaptadorCliente
    {
        Task<ResultadoApi> InsertarAsync(ClienteDto cliente);
        Task<List<ClienteDto>> ObtenerTodoAsync();
        Task<ResultadoApi> ActualizarAsync(ClienteDto cliente);
        Task<ResultadoApi> EliminarAsync(int id, int idUsuario);
        Task<ClienteDto?> ObtenerPorCiAsync(string ci);
        Task<List<ClienteDto>> ObtenerSimilaresPorCiAsync(string ci);
    }
}

