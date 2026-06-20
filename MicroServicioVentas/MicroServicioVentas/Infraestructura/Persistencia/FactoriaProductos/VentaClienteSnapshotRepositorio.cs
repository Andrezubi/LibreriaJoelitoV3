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
                    RazonSocialCliente,
                    CiCliente,
                    ComplementoCliente,
                    EmailCliente,
                    ClienteFrecuente
                )
                VALUES (
                    @idVenta,
                    @idCliente,
                    @razonSocialCliente,
                    @ciCliente,
                    @complementoCliente,
                    @emailCliente,
                    @clienteFrecuente
                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idVenta", snapshot.IdVenta);
            comando.Parameters.AddWithValue("@idCliente", snapshot.IdCliente);
            comando.Parameters.AddWithValue("@razonSocialCliente", snapshot.RazonSocialCliente);
            comando.Parameters.AddWithValue("@ciCliente", snapshot.CiCliente);
            comando.Parameters.AddWithValue("@complementoCliente", snapshot.ComplementoCliente);
            comando.Parameters.AddWithValue("@emailCliente", snapshot.EmailCliente);
            comando.Parameters.AddWithValue("@clienteFrecuente", snapshot.ClienteFrecuente);

            return ExecuteNonQuery(comando);
        }

        public int Actualizar(VentaClienteSnapshot snapshot)
        {
            string consulta = @"
                UPDATE venta_cliente_snapshot
                SET RazonSocialCliente = @razonSocialCliente,
                    CiCliente = @ciCliente,
                    ComplementoCliente = @complementoCliente,
                    EmailCliente = @emailCliente,
                    ClienteFrecuente = @clienteFrecuente
                WHERE Id = @id;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@razonSocialCliente", snapshot.RazonSocialCliente);
            comando.Parameters.AddWithValue("@ciCliente", snapshot.CiCliente);
            comando.Parameters.AddWithValue("@complementoCliente", snapshot.ComplementoCliente);
            comando.Parameters.AddWithValue("@emailCliente", snapshot.EmailCliente);
            comando.Parameters.AddWithValue("@clienteFrecuente", snapshot.ClienteFrecuente);
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
                       RazonSocialCliente,
                       CiCliente,
                       ComplementoCliente,
                       EmailCliente,
                       ClienteFrecuente,
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
                       RazonSocialCliente,
                       CiCliente,
                       ComplementoCliente,
                       EmailCliente,
                       ClienteFrecuente,
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
                RazonSocialCliente = ObtenerString(reader, "RazonSocialCliente"),
                CiCliente = ObtenerString(reader, "CiCliente"),
                ComplementoCliente = ObtenerStringNullable(reader, "ComplementoCliente"),
                EmailCliente = ObtenerStringNullable(reader, "EmailCliente"),
                ClienteFrecuente = reader.GetBoolean("ClienteFrecuente"),
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