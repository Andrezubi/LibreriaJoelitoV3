using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class VentaRepositorio : RepositorioBD, IRepositorio<Venta>
    {
        public int Insertar(Venta venta)
        {
            string consulta = @"
                INSERT INTO venta (
                    CorrelationId,
                    IdCliente,
                    IdUsuario,
                    Total,
                    Estado,
                    MotivoFallo
                )
                VALUES (
                    @correlationId,
                    @idCliente,
                    @idUsuario,
                    @total,
                    @estado,
                    @motivoFallo
                );
                SELECT LAST_INSERT_ID();";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@correlationId", venta.CorrelationId);
            comando.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
            comando.Parameters.AddWithValue("@total", venta.Total);
            comando.Parameters.AddWithValue("@estado", venta.Estado);
            comando.Parameters.AddWithValue("@motivoFallo", venta.MotivoFallo);

            return Convert.ToInt32(ExecuteScalar(comando));
        }

        public int Actualizar(Venta venta)
        {
            string consulta = @"
                UPDATE venta
                SET IdCliente = @idCliente,
                    IdUsuario = @idUsuario,
                    Total = @total,
                    Estado = @estado,
                    MotivoFallo = @motivoFallo
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
            comando.Parameters.AddWithValue("@total", venta.Total);
            comando.Parameters.AddWithValue("@estado", venta.Estado);
            comando.Parameters.AddWithValue("@motivoFallo", venta.MotivoFallo);
            comando.Parameters.AddWithValue("@id", venta.Id);

            return ExecuteNonQuery(comando);
        }

        public int Eliminar(Venta venta)
        {
            string consulta = @"
                UPDATE venta
                SET Estado = @estado
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", EstadosVenta.Anulada);
            comando.Parameters.AddWithValue("@id", venta.Id);

            return ExecuteNonQuery(comando);
        }

        public List<Venta> ObtenerTodo()
        {
            string consulta = @"
                SELECT 
                    Id,
                    CorrelationId,
                    IdCliente,
                    IdUsuario,
                    Fecha,
                    Total,
                    Estado,
                    MotivoFallo,
                    FechaRegistro,
                    FechaUltimaActualizacion
                FROM venta
                WHERE Estado <> @estadoAnulada
                ORDER BY Fecha DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@estadoAnulada", EstadosVenta.Anulada);

            var ventas = new List<Venta>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                ventas.Add(MapearVenta(reader));
            }

            return ventas;
        }

        public Venta? ObtenerPorId(int id)
        {
            string consulta = @"
                SELECT 
                    Id,
                    CorrelationId,
                    IdCliente,
                    IdUsuario,
                    Fecha,
                    Total,
                    Estado,
                    MotivoFallo,
                    FechaRegistro,
                    FechaUltimaActualizacion
                FROM venta
                WHERE Id = @id
                LIMIT 1;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@id", id);

            var reader = ExecuteReader(comando);

            if (!reader.Read())
                return null;

            return MapearVenta(reader);
        }

        public Venta? ObtenerPorCorrelationId(string correlationId)
        {
            string consulta = @"
                SELECT 
                    Id,
                    CorrelationId,
                    IdCliente,
                    IdUsuario,
                    Fecha,
                    Total,
                    Estado,
                    MotivoFallo,
                    FechaRegistro,
                    FechaUltimaActualizacion
                FROM venta
                WHERE CorrelationId = @correlationId
                LIMIT 1;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@correlationId", correlationId);

            var reader = ExecuteReader(comando);

            if (!reader.Read())
                return null;

            return MapearVenta(reader);
        }

        public int ActualizarEstadoPorCorrelationId(
            string correlationId,
            string estado,
            string? motivoFallo = null)
        {
            string consulta = @"
                UPDATE venta
                SET Estado = @estado,
                    MotivoFallo = @motivoFallo
                WHERE CorrelationId = @correlationId;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", estado);
            comando.Parameters.AddWithValue("@motivoFallo", motivoFallo);
            comando.Parameters.AddWithValue("@correlationId", correlationId);

            return ExecuteNonQuery(comando);
        }

        public int ActualizarEstadoPorId(
            int idVenta,
            string estado,
            string? motivoFallo = null)
        {
            string consulta = @"
                UPDATE venta
                SET Estado = @estado,
                    MotivoFallo = @motivoFallo
                WHERE Id = @idVenta;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", estado);
            comando.Parameters.AddWithValue("@motivoFallo", motivoFallo);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            return ExecuteNonQuery(comando);
        }

        private Venta MapearVenta(MySqlDataReader reader)
        {
            return new Venta
            {
                Id = reader.GetInt32("Id"),
                CorrelationId = reader.GetString("CorrelationId"),
                IdCliente = reader.GetInt32("IdCliente"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Fecha = reader.GetDateTime("Fecha"),
                Total = reader.GetDecimal("Total"),
                Estado = reader.GetString("Estado"),
                MotivoFallo = reader.IsDBNull(reader.GetOrdinal("MotivoFallo"))
                    ? null
                    : reader.GetString("MotivoFallo"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaUltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                    ? null
                    : reader.GetDateTime("FechaUltimaActualizacion")
            };
        }
    }
}