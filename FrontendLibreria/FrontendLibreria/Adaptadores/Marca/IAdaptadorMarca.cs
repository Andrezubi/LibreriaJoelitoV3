using FrontendLibreria.DTOs;

namespace FrontendLibreria.Adaptadores.Marca
{
    public interface IAdaptadorMarca
    {
            Task<List<MarcaDto>> ObtenerTodoAsync();
            Task<ResultadoProductoApi> InsertarAsync(MarcaDto marca);
            Task<ResultadoProductoApi> ActualizarAsync(MarcaDto marca);
            Task<ResultadoApi> EliminarAsync(int id, int idUsuario);
    }
}
