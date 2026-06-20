using MySql.Data.MySqlClient;
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
                    Cantidad,
                    PrecioUnitario,
                    Estado
                )
                VALUES (
                    @idVenta,
                    @idProducto,
                    @idPresentacion,
                    @cantidad,
                    @precioUnitario,
                    @estado
                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", detalleVenta.IdVenta);
            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);
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
                    Cantidad = @cantidad,
                    PrecioUnitario = @precioUnitario,
                    Estado = @estado
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            comando.Parameters.AddWithValue("@idPresentacion", detalleVenta.IdPresentacion);
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
                SELECT
                    Id,
                    IdVenta,
                    IdProducto,
                    IdPresentacion,
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
                SELECT
                    Id,
                    IdVenta,
                    IdProducto,
                    IdPresentacion,
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
            string consulta = @"
                UPDATE detalleventa
                SET Estado = @estado
                WHERE IdVenta = @idVenta;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", EstadosDetalleVenta.Liberado);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            return ExecuteNonQuery(comando);
        }

        private DetalleVenta MapearDetalleVenta(MySqlDataReader reader)
        {
            return new DetalleVenta
            {
                Id = reader.GetInt32("Id"),
                IdVenta = reader.GetInt32("IdVenta"),
                IdProducto = reader.GetInt32("IdProducto"),
                IdPresentacion = reader.GetInt32("IdPresentacion"),
                Cantidad = reader.GetInt32("Cantidad"),
                PrecioUnitario = reader.GetDecimal("PrecioUnitario"),
                Subtotal = reader.GetDecimal("Subtotal"),
                Estado = reader.GetString("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaUltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                    ? null
                    : reader.GetDateTime("FechaUltimaActualizacion")
            };
        }
    }
}