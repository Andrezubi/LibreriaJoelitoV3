using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs;
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
                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@correlationId", venta.CorrelationId);
            comando.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
            comando.Parameters.AddWithValue("@total", venta.Total);
            comando.Parameters.AddWithValue("@estado", venta.Estado);
            comando.Parameters.AddWithValue("@motivoFallo", venta.MotivoFallo);

            int filas = ExecuteNonQuery(comando);

            if (filas <= 0)
                return 0;

            return Convert.ToInt32(comando.LastInsertedId);
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
            return ActualizarEstadoPorId(venta.Id, venta.IdUsuario, EstadosVenta.Anulada);
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

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                ventas.Add(MapearVenta(reader));
            }

            return ventas;
        }

        public List<VentaDTO> ObtenerResumenVentas()
        {
            string consulta = @"
                SELECT 
                    v.Id,
                    v.CorrelationId,
                    v.IdCliente,
                    COALESCE(s.RazonSocialCliente, '') AS RazonSocialCliente,
                    COALESCE(s.CiCliente, '') AS CiCliente,
                    v.Fecha,
                    v.Total,
                    v.Estado
                FROM venta v
                LEFT JOIN venta_cliente_snapshot s ON s.IdVenta = v.Id
                WHERE v.Estado <> @estadoStockRechazado
                ORDER BY v.Fecha DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estadoStockRechazado", EstadosVenta.StockRechazado);

            var ventas = new List<VentaDTO>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                ventas.Add(new VentaDTO
                {
                    Id = reader.GetInt32("Id"),
                    CorrelationId = ObtenerString(reader, "CorrelationId"),
                    IdCliente = reader.GetInt32("IdCliente"),
                    RazonSocialCliente = ObtenerString(reader, "RazonSocialCliente"),
                    CiCliente = ObtenerString(reader, "CiCliente"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = reader.GetDecimal("Total"),
                    Estado = ObtenerString(reader, "Estado")
                });
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

            using var reader = ExecuteReader(comando);

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

            using var reader = ExecuteReader(comando);

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
            int idUsuario,
            string estado,
            string? motivoFallo = null)
        {
            string consulta = @"
                UPDATE venta
                SET Estado = @estado,
                    MotivoFallo = @motivoFallo,
                    IdUsuario = @idUsuario
                WHERE Id = @idVenta;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@estado", estado);
            comando.Parameters.AddWithValue("@motivoFallo", motivoFallo);
            comando.Parameters.AddWithValue("@idVenta", idVenta);
            comando.Parameters.AddWithValue("@idUsuario", idUsuario);

            return ExecuteNonQuery(comando);
        }

        private Venta MapearVenta(MySqlDataReader reader)
        {
            return new Venta
            {
                Id = reader.GetInt32("Id"),
                CorrelationId = ObtenerString(reader, "CorrelationId"),
                IdCliente = reader.GetInt32("IdCliente"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Fecha = reader.GetDateTime("Fecha"),
                Total = reader.GetDecimal("Total"),
                Estado = ObtenerString(reader, "Estado"),
                MotivoFallo = ObtenerStringNullable(reader, "MotivoFallo"),
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