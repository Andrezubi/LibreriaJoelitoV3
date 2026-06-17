using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class FachadaAnularVenta 
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly ProductoRepositorio _productoRepositorio;

        public FachadaAnularVenta(
            ProductoRepositorio productoRepositorio,
            VentaRepositorio ventaRepositorio,
            DetalleVentaRepositorio detalleVentaRepositorio)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _productoRepositorio = productoRepositorio;
        }

        public Result<int> AnularVenta(int idVenta, int idEmpleado)
        {
            try
            {
                var ventaFila = _ventaRepositorio.ObtenerPorId(idVenta);

                if (ventaFila == null)
                    return Result<int>.Failure("La venta ya ha sido anulada antes.");

                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    List<DetalleVentaStockDTO> detalles = _detalleVentaRepositorio.ObtenerPorIdVenta(idVenta);

                    if (detalles == null || detalles.Count == 0)
                    {
                        RepositorioBD.Instancia.Rollback();
                        return Result<int>.Failure("No se encontraron detalles para la venta.");
                    }

                    foreach (DetalleVentaStockDTO detalle in detalles)
                    {
                        int idProducto = detalle.IdProducto;
                        int cantidad = Convert.ToInt32(detalle.Cantidad * detalle.FactorConversion);

                        int filasStock = _productoRepositorio.RestaurarStock(idProducto, cantidad);

                        if (filasStock <= 0)
                        {
                            RepositorioBD.Instancia.Rollback();
                            return Result<int>.Failure($"Error al restaurar el stock del producto ID {idProducto}.");
                        }
                    }

                    Venta venta = new Venta
                    {
                        Id = idVenta,
                        IdUsuario = idEmpleado
                    };

                    int resultado = _ventaRepositorio.Eliminar(venta);

                    if (resultado <= 0)
                    {
                        RepositorioBD.Instancia.Rollback();
                        return Result<int>.Failure("No se pudo actualizar el estado de la venta.");
                    }

                    RepositorioBD.Instancia.Commit();

                    return Result<int>.Success(venta.Id);
                }
                catch (Exception ex)
                {
                    RepositorioBD.Instancia.Rollback();
                    return Result<int>.Failure($"Transacción revertida. Error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Error inesperado al anular: {ex.Message}");
            }
        }
    }
}