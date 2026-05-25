using MySql.Data.MySqlClient;
using MicroServicioClientes.Aplicacion.Interfaces;
using MicroServicioClientes.Dominio.Modelos;
using MicroServicioClientes.Infrestructura.Persistencia;
using System.Data;

namespace MicroServicioClientes.Infrestructura.Persistencia.FactoriaProductos
{
    public class ClienteRepositorio : RepositorioBD, IRepositorio<Cliente>
    {
        public int Eliminar(Cliente t)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE Cliente SET
                    Estado                   = 0,
                    IdUsuario                = @idUsuario,
                    FechaUltimaActualizacion = NOW()
                WHERE Id = @id");

            cmd.Parameters.AddWithValue("@idUsuario", t.IdUsuario);
            cmd.Parameters.AddWithValue("@id", t.Id);
            return ExecuteNonQuery(cmd);
        }

        public List<Cliente> ObtenerTodo()
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT Id, RazonSocial, Ci, Complemento, 
                       Email, ClienteFrecuente, FechaRegistro
                FROM Cliente
                WHERE Estado = 1
                ORDER BY RazonSocial");

            var result = new List<Cliente>();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result.Add(new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    RazonSocial = reader["RazonSocial"].ToString()!,
                    Ci = reader["Ci"].ToString()!,
                    Complemento = reader["Complemento"] == DBNull.Value ? null : reader["Complemento"].ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    ClienteFrecuente = reader["ClienteFrecuente"] != DBNull.Value && Convert.ToBoolean(reader["ClienteFrecuente"]),
                    FechaRegistro = (DateTime)reader["FechaRegistro"]
                });
            }
            return result;
        }

        public Cliente ObtenerPorId(int id)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT Id, RazonSocial, Ci, Complemento, 
                       Email, ClienteFrecuente, FechaRegistro
                FROM Cliente
                WHERE Id = @id AND Estado = 1");

            cmd.Parameters.AddWithValue("@id", id);

            var result = new Cliente();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result = new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    RazonSocial = reader["RazonSocial"].ToString()!,
                    Ci = reader["Ci"].ToString()!,
                    Complemento = reader["Complemento"] == DBNull.Value ? null : reader["Complemento"].ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    ClienteFrecuente = (bool)reader["ClienteFrecuente"],
                    FechaRegistro = (DateTime)reader["FechaRegistro"]
                };
            }
            return result;
        }

        public DataRow ObtenerPorIdDR(int id)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT Id, RazonSocial,
                       Ci AS Ci, Complemento, Email, ClienteFrecuente AS ClienteFrecuente, FechaRegistro
                FROM Cliente
                WHERE Id = @id AND Estado = 1");

            cmd.Parameters.AddWithValue("@id", id);

            return ExecuteReturningDataRow(cmd);
        }

        public Cliente ObtenerPorCi(string ci)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT Id, RazonSocial, Ci, Complemento, 
                       Email, ClienteFrecuente, FechaRegistro
                FROM Cliente
                WHERE Ci = @ci AND Estado = 1
                LIMIT 1");

            cmd.Parameters.AddWithValue("@ci", ci);

            var result = new Cliente();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result = new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    RazonSocial = reader["RazonSocial"].ToString()!,
                    Ci = reader["Ci"].ToString()!,
                    Complemento = reader["Complemento"] == DBNull.Value ? null : reader["Complemento"].ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    ClienteFrecuente = (bool)reader["ClienteFrecuente"],
                    FechaRegistro = (DateTime)reader["FechaRegistro"]
                };
            }
            return result;
        }

        public int Insertar(Cliente t)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO Cliente 
                    (RazonSocial, Ci, Complemento, Email, ClienteFrecuente, IdUsuario)
                VALUES 
                    (@razonSocial, @ci, @complemento, @email, @clienteFrecuente, @idUsuario);
                SELECT LAST_INSERT_ID();");

            AgregarParametros(cmd, t);
            return Convert.ToInt32(ExecuteScalar(cmd));
        }

        public int Actualizar(Cliente t)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE Cliente SET
                    RazonSocial              = @razonSocial,
                    Ci                       = @ci,
                    Complemento              = @complemento,
                    Email                    = @email,
                    ClienteFrecuente         = @clienteFrecuente,
                    IdUsuario                = @idUsuario,
                    FechaUltimaActualizacion = NOW()
                WHERE Id = @id");

            AgregarParametros(cmd, t);
            cmd.Parameters.AddWithValue("@id", t.Id);
            return ExecuteNonQuery(cmd);
        }

        public bool ExisteDuplicado(Cliente cliente)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Cliente
                WHERE Ci          = @ci
                  AND Complemento = @complemento
                  AND Id         <> @id
                  AND Estado      = 1");

            cmd.Parameters.AddWithValue("@ci", cliente.Ci);
            cmd.Parameters.AddWithValue("@complemento", cliente.Complemento ?? string.Empty);
            cmd.Parameters.AddWithValue("@id", cliente.Id);
            return Convert.ToInt32(ExecuteScalar(cmd)) > 0;
        }

        public List<Cliente> ObtenerSimilarCi(string ci)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT Id, RazonSocial, Ci, Complemento
                FROM Cliente
                WHERE Estado = 1 
                AND CONCAT(Ci, IFNULL(Complemento,'')) LIKE @ci
                LIMIT 10");

            cmd.Parameters.AddWithValue("@ci", "%" + ci + "%");

            var result = new List<Cliente>();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result.Add(new Cliente
                {
                    Id = reader.GetInt32("Id"),
                    RazonSocial = reader["RazonSocial"].ToString()!,
                    Ci = reader["Ci"].ToString()!,
                    Complemento = reader["Complemento"] == DBNull.Value ? null : reader["Complemento"].ToString()
                });
            }
            return result;
        }

        // --- Métodos privados de apoyo ---

        private static void AgregarParametros(MySqlCommand cmd, Cliente cliente)
        {
            cmd.Parameters.AddWithValue("@razonSocial", cliente.RazonSocial);
            cmd.Parameters.AddWithValue("@ci", cliente.Ci);
            cmd.Parameters.AddWithValue("@complemento", cliente.Complemento ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", (object?)cliente.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@clienteFrecuente", cliente.ClienteFrecuente);
            cmd.Parameters.AddWithValue("@idUsuario", cliente.IdUsuario);
        }
    }
}