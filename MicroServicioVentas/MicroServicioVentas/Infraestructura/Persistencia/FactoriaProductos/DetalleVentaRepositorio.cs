using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs.ServicioVentaDTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using System.Data;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class DetalleVentaRepositorio : RepositorioBD, IRepositorio<DetalleVenta>
    {
        public int Insertar(DetalleVenta detalleVenta)
        {
            string consulta = @"INSERT INTO detalleventa ( IdVenta, IdProducto, IdPresentacion, Cantidad, PrecioUnitario, Subtotal)
                                VALUES (@idVenta, @idProducto, @idPresentacion, @cantidad, @precioUnitario, @subtotal);";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", detalleVenta.IdVenta);
            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);
            comando.Parameters.AddWithValue("@cantidad", detalleVenta.Cantidad);
            comando.Parameters.AddWithValue("@precioUnitario", detalleVenta.PrecioUnitario);
            comando.Parameters.AddWithValue("@subtotal", detalleVenta.Subtotal);
            return ExecuteNonQuery(comando);
        }

        public int Actualizar(DetalleVenta detalleVenta)
        {
            throw new NotImplementedException();
        }

        public List<DetalleVenta> ObtenerTodo()
        {
            string consulta = @"SELECT * 
                                FROM detalleventa";
            MySqlCommand comando = new MySqlCommand(consulta);

            var result = new List<DetalleVenta>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(
                    new DetalleVenta
                    {
                        IdVenta = reader.GetInt32("IdVenta"),
                        IdProducto = reader.GetInt32("IdProducto"),
                        IdPresentacion = reader.GetInt32("IdPresentacion"),
                        Cantidad = reader.GetInt32("Cantidad"),
                        PrecioUnitario = reader.GetDecimal("PrecioUnitario")
                    }
                    );
            }

            return result;
        }

        public int Eliminar(DetalleVenta detalleVenta)
        {
            string consulta = @"DELETE FROM detalleventa
                                WHERE IdVenta = @idVenta
                                    AND IdProducto = @idProducto
                                    AND IdPresentacion = @idPresentacion";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", detalleVenta.IdVenta);
            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);

            return ExecuteNonQuery(comando);
        }

        public List<DetalleVentaStockDTO> ObtenerPorIdVenta(int idVenta)
        {
            string consulta = @"SELECT dv.IdProducto AS IdProducto,
                               dv.Cantidad AS Cantidad,
                               pp.FactorConversion AS FactorConversion
                        FROM detalleventa dv
                        INNER JOIN presentacionproducto pp 
                            ON dv.IdPresentacion = pp.IdPresentacion 
                            AND dv.IdProducto = pp.IdProducto
                        WHERE dv.IdVenta = @idVenta";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            var resultado = new List<DetalleVentaStockDTO>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                resultado.Add(new DetalleVentaStockDTO
                {
                    IdProducto = reader.GetInt32("IdProducto"),
                    Cantidad = reader.GetInt32("Cantidad"),
                    FactorConversion = reader.GetInt32("FactorConversion")
                });
            }

            return resultado;
        }

        public List<DetalleVentaExtraDTO> ObtenerDetalleExtraPorIdVenta(int idVenta)
        {
            string consulta = @"SELECT dv.IdVenta AS IdVenta, pr.Nombre AS NombreProducto, prs.Nombre AS NombrePresentacion, 
                                    dv.Cantidad AS Cantidad, dv.PrecioUnitario AS PrecioUnitario, dv.Subtotal AS Subtotal, 
                                    pp.FactorConversion AS FactorConversion FROM detalleventa dv
                                INNER JOIN presentacionproducto pp ON dv.IdPresentacion = pp.IdPresentacion AND dv.IdProducto = pp.IdProducto
                                INNER JOIN producto pr ON dv.IdProducto = pr.Id
							    INNER JOIN presentacion prs ON dv.IdPresentacion = prs.Id
                                WHERE dv.IdVenta = @idVenta";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", idVenta);

            var resultado = new List<DetalleVentaExtraDTO>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                resultado.Add(new DetalleVentaExtraDTO
                {
                    IdVenta = reader.GetInt32("IdVenta"),
                    Producto = reader.GetString("NombreProducto"),
                    Presentacion = reader.GetString("NombrePresentacion"),
                    Cantidad = reader.GetInt32("Cantidad"),
                    PrecioUnitario = reader.GetDecimal("PrecioUnitario"),
                    Subtotal = reader.GetDecimal("Subtotal")
                });
            }

            return resultado;
        }

        public int EliminarPorIdVenta(int idVenta)
        {
            string consulta = @"DELETE FROM detalleventa
                                WHERE IdVenta = @idVenta";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", idVenta);

            return ExecuteNonQuery(comando);
        }
    }
}
