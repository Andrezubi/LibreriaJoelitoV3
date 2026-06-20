using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class DetalleVentaRepositorio : RepositorioBD, IRepositorio<DetalleVenta>
    {
        public int Insertar(DetalleVenta detalleVenta)
        {
            string consulta = @"
                INSERT INTO detalleventa (
                    IdVenta,
                    IdProducto,
                    IdPresentacion,
                    NombreProducto,
                    NombrePresentacion,
                    CodigoProducto,
                    UnidadPresentacion,
                    Cantidad,
                    PrecioUnitario,
                    Estado
                )
                VALUES (
                    @idVenta,
                    @idProducto,
                    @idPresentacion,
                    @nombreProducto,
                    @nombrePresentacion,
                    @codigoProducto,
                    @unidadPresentacion,
                    @cantidad,
                    @precioUnitario,
                    @estado
                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", detalleVenta.IdVenta);
            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);
            comando.Parameters.AddWithValue("@nombreProducto", detalleVenta.NombreProducto);
            comando.Parameters.AddWithValue("@nombrePresentacion", detalleVenta.NombrePresentacion);
            comando.Parameters.AddWithValue("@codigoProducto", detalleVenta.CodigoProducto);
            comando.Parameters.AddWithValue("@unidadPresentacion", detalleVenta.UnidadPresentacion);
            comando.Parameters.AddWithValue("@cantidad", detalleVenta.Cantidad);
            comando.Parameters.AddWithValue("@precioUnitario", detalleVenta.PrecioUnitario);
            comando.Parameters.AddWithValue("@estado", detalleVenta.Estado);

            return ExecuteNonQuery(comando);
        }

        public int Actualizar(DetalleVenta detalleVenta)
        {
            string consulta = @"
                UPDATE detalleventa
                SET IdProducto = @idProducto,
                    IdPresentacion = @idPresentacion,
                    NombreProducto = @nombreProducto,
                    NombrePresentacion = @nombrePresentacion,
                    CodigoProducto = @codigoProducto,
                    UnidadPresentacion = @unidadPresentacion,
                    Cantidad = @cantidad,
                    PrecioUnitario = @precioUnitario,
                    Estado = @estado
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);
            comando.Parameters.AddWithValue("@nombreProducto", detalleVenta.NombreProducto);
            comando.Parameters.AddWithValue("@nombrePresentacion", detalleVenta.NombrePresentacion);
            comando.Parameters.AddWithValue("@codigoProducto", detalleVenta.CodigoProducto);
            comando.Parameters.AddWithValue("@unidadPresentacion", detalleVenta.UnidadPresentacion);
            comando.Parameters.AddWithValue("@cantidad", detalleVenta.Cantidad);
            comando.Parameters.AddWithValue("@precioUnitario", detalleVenta.PrecioUnitario);
            comando.Parameters.AddWithValue("@estado", detalleVenta.Estado);
            comando.Parameters.AddWithValue("@id", detalleVenta.Id);

            return ExecuteNonQuery(comando);
        }

        public int Eliminar(DetalleVenta detalleVenta)
        {
            string consulta = @"
                UPDATE detalleventa
                SET Estado = @estado
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", EstadosDetalleVenta.Liberado);
            comando.Parameters.AddWithValue("@id", detalleVenta.Id);

            return ExecuteNonQuery(comando);
        }

        public List<DetalleVenta> ObtenerTodo()
        {
            string consulta = @"
                SELECT Id,
                       IdVenta,
                       IdProducto,
                       IdPresentacion,
                       NombreProducto,
                       NombrePresentacion,
                       CodigoProducto,
                       UnidadPresentacion,
                       Cantidad,
                       PrecioUnitario,
                       Subtotal,
                       Estado,
                       FechaRegistro,
                       FechaUltimaActualizacion
                FROM detalleventa
                ORDER BY Id DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);

            var detalles = new List<DetalleVenta>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                detalles.Add(MapearDetalleVenta(reader));
            }

            return detalles;
        }

        public List<DetalleVenta> ObtenerPorIdVenta(int idVenta)
        {
            string consulta = @"
                SELECT Id,
                       IdVenta,
                       IdProducto,
                       IdPresentacion,
                       NombreProducto,
                       NombrePresentacion,
                       CodigoProducto,
                       UnidadPresentacion,
                       Cantidad,
                       PrecioUnitario,
                       Subtotal,
                       Estado,
                       FechaRegistro,
                       FechaUltimaActualizacion
                FROM detalleventa
                WHERE IdVenta = @idVenta
                ORDER BY Id ASC;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            var detalles = new List<DetalleVenta>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                detalles.Add(MapearDetalleVenta(reader));
            }

            return detalles;
        }

        public List<DetalleVentaExtraDTO> ObtenerDetalleExtraPorIdVenta(int idVenta)
        {
            string consulta = @"
                SELECT IdVenta,
                       IdProducto,
                       IdPresentacion,
                       NombreProducto,
                       NombrePresentacion,
                       CodigoProducto,
                       UnidadPresentacion,
                       Cantidad,
                       PrecioUnitario,
                       Subtotal,
                       Estado
                FROM detalleventa
                WHERE IdVenta = @idVenta
                ORDER BY Id ASC;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            var detalles = new List<DetalleVentaExtraDTO>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                detalles.Add(new DetalleVentaExtraDTO
                {
                    IdVenta = reader.GetInt32("IdVenta"),
                    IdProducto = reader.GetInt32("IdProducto"),
                    IdPresentacion = reader.GetInt32("IdPresentacion"),
                    Producto = ObtenerString(reader, "NombreProducto"),
                    Presentacion = ObtenerString(reader, "NombrePresentacion"),
                    CodigoProducto = ObtenerStringNullable(reader, "CodigoProducto"),
                    UnidadPresentacion = ObtenerStringNullable(reader, "UnidadPresentacion"),
                    Cantidad = reader.GetInt32("Cantidad"),
                    PrecioUnitario = reader.GetDecimal("PrecioUnitario"),
                    Subtotal = reader.GetDecimal("Subtotal"),
                    Estado = ObtenerString(reader, "Estado")
                });
            }

            return detalles;
        }

        public int ActualizarEstadoPorVenta(int idVenta, string estado)
        {
            string consulta = @"
                UPDATE detalleventa
                SET Estado = @estado
                WHERE IdVenta = @idVenta;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", estado);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            return ExecuteNonQuery(comando);
        }

        public int EliminarPorIdVenta(int idVenta)
        {
            return ActualizarEstadoPorVenta(idVenta, EstadosDetalleVenta.Liberado);
        }

        public List<Reporte1DTO> ObtenerReporteServicios()
        {
            string consulta = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY SUM(Subtotal) DESC) AS Nro,
                    NombreProducto,
                    NombrePresentacion,
                    SUM(Cantidad) AS CantidadVendida,
                    SUM(Subtotal) AS TotalVendidoBs,
                    Estado
                FROM detalleventa
                GROUP BY NombreProducto, NombrePresentacion, Estado
                ORDER BY TotalVendidoBs DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);
            var reporte = new List<Reporte1DTO>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                reporte.Add(new Reporte1DTO
                {
                    Nro = Convert.ToInt32(reader["Nro"]),
                    NombreProducto = ObtenerString(reader, "NombreProducto"),
                    Presentacion = ObtenerString(reader, "NombrePresentacion"),
                    CantidadVendida = Convert.ToInt32(reader["CantidadVendida"]),
                    TotalVendidoBs = Convert.ToDecimal(reader["TotalVendidoBs"]),
                    EstadoDetalle = ObtenerString(reader, "Estado")
                });
            }

            return reporte;
        }

        private DetalleVenta MapearDetalleVenta(MySqlDataReader reader)
        {
            return new DetalleVenta
            {
                Id = reader.GetInt32("Id"),
                IdVenta = reader.GetInt32("IdVenta"),
                IdProducto = reader.GetInt32("IdProducto"),
                IdPresentacion = reader.GetInt32("IdPresentacion"),
                NombreProducto = ObtenerString(reader, "NombreProducto"),
                NombrePresentacion = ObtenerString(reader, "NombrePresentacion"),
                CodigoProducto = ObtenerStringNullable(reader, "CodigoProducto"),
                UnidadPresentacion = ObtenerStringNullable(reader, "UnidadPresentacion"),
                Cantidad = reader.GetInt32("Cantidad"),
                PrecioUnitario = reader.GetDecimal("PrecioUnitario"),
                Subtotal = reader.GetDecimal("Subtotal"),
                Estado = ObtenerString(reader, "Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaUltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                    ? null
                    : reader.GetDateTime("FechaUltimaActualizacion")
            };
        }

        private string ObtenerString(MySqlDataReader reader, string columna)
        {
            var valor = reader[columna];

            if (valor == null || valor == DBNull.Value)
                return string.Empty;

            return valor.ToString() ?? string.Empty;
        }

        private string? ObtenerStringNullable(MySqlDataReader reader, string columna)
        {
            var valor = reader[columna];

            if (valor == null || valor == DBNull.Value)
                return null;

            return valor.ToString();
        }
    }
}
