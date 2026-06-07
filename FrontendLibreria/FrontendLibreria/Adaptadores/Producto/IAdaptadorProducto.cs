using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Http;

namespace FrontendLibreria.Adaptadores.Producto
{
    public interface IAdaptadorProducto
    {
        Task<List<ProductoDto>> GetAllAsync();
        Task<List<CategoriaDto>> GetCategoriasAsync();
        Task<List<MarcaDto>> GetMarcasAsync();
        Task<List<PresentacionDto>> GetPresentacionesAsync();
        Task<ResultadoProductoApi> CrearProductoAsync(ProductoDto producto,int idPresentacion,int FactorConversion, decimal precioVenta);
        Task<ResultadoProductoApi> UpdateAsync(ProductoDto producto);
        Task<ResultadoApi> DeleteAsync(int id, int idUsuario);
        Task<ResultadoApi> AgregarPresentacionAsync(SolicitudAgregarPresentacion request);


        Task<ResultadoApi> CrearCategoriaAsync(string nombre, int idUsuario);
    }
}
