using FrontendLibreria.DTOs;
using FrontendLibreria.DTOs.VentaDTOs;

namespace FrontendLibreria.Adaptadores.Producto
{
    public interface IAdaptadorProducto
    {
        Task<List<ProductoDto>> GetAllAsync();

        Task<List<CategoriaDto>> GetCategoriasAsync();

        Task<List<MarcaDto>> GetMarcasAsync();

        Task<List<PresentacionDto>> GetPresentacionesAsync();

        Task<ResultadoProductoApi> CrearProductoAsync(
            ProductoDto producto,
            int idPresentacion,
            int factorConversion,
            decimal precioVenta
        );

        Task<ResultadoProductoApi> UpdateAsync(ProductoDto producto);

        Task<ResultadoApi> DeleteAsync(int id, int idUsuario);

        Task<ResultadoApi> AgregarPresentacionAsync(SolicitudAgregarPresentacion request);

        Task<ResultadoApi> CrearCategoriaAsync(string nombre, int idUsuario);

        Task<List<PresentacionProductoVentaDTO>> ObtenerPresentacionesPorFraseAsync(string frase);

        Task<PresentacionProductoVentaDTO?> ObtenerPresentacionProductoByIdsAsync(
            int idProducto,
            int idPresentacion
        );
    }
}