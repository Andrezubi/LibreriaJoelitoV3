using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class VentaClienteSnapshotRepositorio : RepositorioBD, IRepositorio<VentaClienteSnapshot>
    {
        public int Insertar(VentaClienteSnapshot snapshot)
        {
            string consulta = @"
                INSERT INTO venta_cliente_snapshot (
                    IdVenta,
                    IdCliente,
                    NombreCliente,
                    DocumentoCliente,
                    NitCliente,
                    TelefonoCliente,
                    DireccionCliente
                )
                VALUES (
                    @idVenta,
                    @idCliente,
                    @nombreCliente,
                    @documentoCliente,
                    @nitCliente,
                    @telefonoCliente,
                    @direccionCliente
                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", snapshot.IdVenta);
            comando.Parameters.AddWithValue("@idCliente", snapshot.IdCliente);
            comando.Parameters.AddWithValue("@nombreCliente", snapshot.NombreCliente);
            comando.Parameters.AddWithValue("@documentoCliente", snapshot.DocumentoCliente);
            comando.Parameters.AddWithValue("@nitCliente", snapshot.NitCliente);
            comando.Parameters.AddWithValue("@telefonoCliente", snapshot.TelefonoCliente);
            comando.Parameters.AddWithValue("@direccionCliente", snapshot.DireccionCliente);

            return ExecuteNonQuery(comando);
        }

        public int Actualizar(VentaClienteSnapshot snapshot)
        {
            string consulta = @"
                UPDATE venta_cliente_snapshot
                SET NombreCliente = @nombreCliente,
                    DocumentoCliente = @documentoCliente,
                    NitCliente = @nitCliente,
                    TelefonoCliente = @telefonoCliente,
                    DireccionCliente = @direccionCliente
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@nombreCliente", snapshot.NombreCliente);
            comando.Parameters.AddWithValue("@documentoCliente", snapshot.DocumentoCliente);
            comando.Parameters.AddWithValue("@nitCliente", snapshot.NitCliente);
            comando.Parameters.AddWithValue("@telefonoCliente", snapshot.TelefonoCliente);
            comando.Parameters.AddWithValue("@direccionCliente", snapshot.DireccionCliente);
            comando.Parameters.AddWithValue("@id", snapshot.Id);

            return ExecuteNonQuery(comando);
        }

        public int Eliminar(VentaClienteSnapshot snapshot)
        {
            string consulta = @"DELETE FROM venta_cliente_snapshot WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@id", snapshot.Id);

            return ExecuteNonQuery(comando);
        }

        public List<VentaClienteSnapshot> ObtenerTodo()
        {
            string consulta = @"
                SELECT Id,
                       IdVenta,
                       IdCliente,
                       NombreCliente,
                       DocumentoCliente,
                       NitCliente,
                       TelefonoCliente,
                       DireccionCliente,
                       FechaRegistro
                FROM venta_cliente_snapshot
                ORDER BY Id DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);

            var snapshots = new List<VentaClienteSnapshot>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                snapshots.Add(MapearSnapshot(reader));
            }

            return snapshots;
        }

        public VentaClienteSnapshot? ObtenerPorIdVenta(int idVenta)
        {
            string consulta = @"
                SELECT Id,
                       IdVenta,
                       IdCliente,
                       NombreCliente,
                       DocumentoCliente,
                       NitCliente,
                       TelefonoCliente,
                       DireccionCliente,
                       FechaRegistro
                FROM venta_cliente_snapshot
                WHERE IdVenta = @idVenta
                LIMIT 1;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            using var reader = ExecuteReader(comando);

            if (!reader.Read())
                return null;

            return MapearSnapshot(reader);
        }

        private VentaClienteSnapshot MapearSnapshot(MySqlDataReader reader)
        {
            return new VentaClienteSnapshot
            {
                Id = reader.GetInt32("Id"),
                IdVenta = reader.GetInt32("IdVenta"),
                IdCliente = reader.GetInt32("IdCliente"),
                NombreCliente = ObtenerString(reader, "NombreCliente"),
                DocumentoCliente = ObtenerStringNullable(reader, "DocumentoCliente"),
                NitCliente = ObtenerStringNullable(reader, "NitCliente"),
                TelefonoCliente = ObtenerStringNullable(reader, "TelefonoCliente"),
                DireccionCliente = ObtenerStringNullable(reader, "DireccionCliente"),
                FechaRegistro = reader.GetDateTime("FechaRegistro")
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
