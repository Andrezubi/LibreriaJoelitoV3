using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.DTOs.ServicioVentaDTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;
using System.Data;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class ConsultaVentaServicio
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        //private readonly IRepository<PresentacionProducto> _presentaProdRepositorio;
        private readonly IPdfServicio _pdfServicio;

        public ConsultaVentaServicio(
            //IRepository<PresentacionProducto> presentProdRepositorio,
            VentaRepositorio ventaRepositorio,
            DetalleVentaRepositorio detalleVentaRepositorio,
            IPdfServicio pdfServicio
        )
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            //_presentaProdRepositorio = presentProdRepositorio;
            _pdfServicio = pdfServicio;
        }

        public List<PresentacionProductoVentaDTO> getPresentacionProductosByFrase(string frase)
        {
            //return _presentaProdRepositorio.obtenerPresentacionProductoDetallado(frase); CAMBIAR CUNADO SE PASE A REPOSITORIO DEDICADO
            return obtenerPresentacionProductoDetallado(frase);
        }

        public List<VentaDTO> CargarVentas()
        {
            return _ventaRepositorio.CargarVentas();
        }

        public Result<PresentacionProductoVentaDTO> GetPresentacionProductoByIds(int idProducto,int idPresentacion)
        {
            try
            {
                //DataRow fila = _presentaProdRepositorio.GetByIds(idProducto, idPresentacion);
                DataRow fila = GetByIds(idProducto, idPresentacion);

                if (fila == null)
                    return Result<PresentacionProductoVentaDTO>.Failure("No se encontró la presentación del producto.");

                var producto = new PresentacionProductoVentaDTO
                {
                    IdProducto = idProducto,
                    IdPresentacion = idPresentacion,
                    Nombre = fila["Descripcion"].ToString() ?? string.Empty,
                    PrecioUnitario = Convert.ToDecimal(fila["Precio"])
                };

                return Result<PresentacionProductoVentaDTO>.Success(producto);
            }
            catch (Exception ex)
            {
                return Result<PresentacionProductoVentaDTO>.Failure($"Error al obtener la presentación del producto: {ex.Message}");
            }
        }

        public Result<byte[]> GenerarComprobantePdf(int idVenta)
        {
            try
            {
                // 1. Pedimos los datos al repositorio (La consulta de los Joins)
                DataTable dt = _ventaRepositorio.ObtenerDatosComprobante(idVenta);

                if (dt == null || dt.Rows.Count == 0)
                    return Result<byte[]>.Failure("No se encontró la venta.");

                // 2. Delegamos la creación del archivo al servicio especializado
                byte[] pdf = _pdfServicio.GenerarComprobanteVenta(dt);

                return Result<byte[]>.Success(pdf);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"Error en fachada de PDF: {ex.Message}");
            }
        }

        public Result<VentaCompletaDTO> ObtenerVentaCompleta(int idVenta)
        {
            try
            {
                VentaCabeceraDTO? venta = _ventaRepositorio.ObtenerCabeceraVentaPorId(idVenta);

                if (venta == null)
                    return Result<VentaCompletaDTO>.Failure("No se encontró la venta.");

                List<DetalleVentaExtraDTO> detalles = _detalleVentaRepositorio.ObtenerDetalleExtraPorIdVenta(idVenta);

                var ventaCompleta = new VentaCompletaDTO
                {
                    Venta = venta,
                    Detalles = detalles
                };

                return Result<VentaCompletaDTO>.Success(ventaCompleta);
            }
            catch (Exception ex)
            {
                return Result<VentaCompletaDTO>.Failure($"Error al obtener la venta completa: {ex.Message}");
            }
        }

        // TODO: Mover a PresentacionProductoRepository
        public DataRow? GetByIds(int idProducto, int idPresentacion)
        {
            string query = @"
                                SELECT 
                                    pp.IdProducto,
                                    pp.IdPresentacion,
                                    pp.Precio,
                                    pp.FactorConversion AS FactorConversion,
                                    p.Nombre AS Producto,
                                    pr.Nombre AS Presentacion,
                                    m.Nombre AS Marca,
                                    CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) AS Descripcion
                                FROM PresentacionProducto pp
                                INNER JOIN Producto p ON pp.IdProducto = p.Id
                                INNER JOIN Presentacion pr ON pp.IdPresentacion = pr.Id
                                LEFT JOIN Marca m ON p.IdMarca = m.Id
                                WHERE pp.IdProducto = @idProducto
                                  AND pp.IdPresentacion = @idPresentacion
                                  AND pp.Estado = 1
                                  AND p.Estado = 1
                                  AND pr.Estado = 1";

            var cmd = new MySqlCommand(query);

            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idPresentacion", idPresentacion);

            var dt = RepositorioBD.Instancia.ExecuteReturningDataTable(cmd);

            if (dt.Rows.Count > 0)
                return dt.Rows[0];

            return null;
        }


        // TODO: Mover a PresentacionProductoRepository
        public List<PresentacionProductoVentaDTO> obtenerPresentacionProductoDetallado(string frase)
        {
            string query = @"SELECT 
                        pp.IdProducto,
                        pp.IdPresentacion,
                        pp.Estado AS EstadoPresentacionProducto,
                        p.Nombre AS Producto,
                        pr.Nombre AS Presentacion,
                        m.Nombre AS Marca,
                        CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) AS Descripcion,
                        pp.Precio
                    FROM PresentacionProducto pp
                    INNER JOIN Producto p 
                        ON pp.IdProducto = p.Id
                    INNER JOIN Presentacion pr 
                        ON pp.IdPresentacion = pr.Id
                    LEFT JOIN Marca m 
                        ON p.IdMarca = m.Id
                    WHERE CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) 
                          LIKE CONCAT('%', @frase, '%')
                      AND pp.Estado = TRUE
                      AND p.Estado = TRUE
                      AND pr.Estado = TRUE
                      AND (m.Estado = TRUE OR m.Id IS NULL);";
            MySqlCommand cmd = new MySqlCommand(query);
            cmd.Parameters.AddWithValue("@frase", frase);
            var resultado = new List<PresentacionProductoVentaDTO>();
            var reader = RepositorioBD.Instancia.ExecuteReader(cmd);
            while (reader.Read()) {
                var dto = new PresentacionProductoVentaDTO
                {
                    IdProducto = reader.GetInt32("IdProducto"),
                    IdPresentacion = reader.GetInt32("IdPresentacion"),
                    EstadoPresentacionProducto = reader.GetBoolean("EstadoPresentacionProducto"),
                    Producto = reader.GetString("Producto"),
                    Presentacion = reader.GetString("Presentacion"),
                    Marca = reader.IsDBNull("Marca") ? null : reader.GetString("Marca"),
                    Descripcion = reader.GetString("Descripcion"),
                    Precio = reader.GetDecimal("Precio")
                };
                resultado.Add(dto);
            }
            return resultado;
        }
        public List<Reporte1DTO> ObtenerReporteServicios()
        {
            return _ventaRepositorio.ObtenerReporteServicios();
        }
    }
}