using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class ProcessedMessageRepositorio : RepositorioBD, IRepositorio<ProcessedMessage>
    {
        public int Insertar(ProcessedMessage mensaje)
        {
            string consulta = @"INSERT INTO ProcessedMessages (
                                    MessageId,
                                    CorrelationId,
                                    RoutingKey
                                )
                                VALUES (
                                    @messageId,
                                    @correlationId,
                                    @routingKey
                                );";

            SqlCommand comando = new SqlCommand(consulta);

            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);
            comando.Parameters.AddWithValue("@correlationId", mensaje.CorrelationId);
            comando.Parameters.AddWithValue("@routingKey", mensaje.RoutingKey);

            return ExecuteNonQuery(comando);
        }

        public int Actualizar(ProcessedMessage mensaje)
        {
            return 0;
        }

        public int Eliminar(ProcessedMessage mensaje)
        {
            string consulta = @"DELETE FROM ProcessedMessages
                                WHERE MessageId = @messageId;";

            SqlCommand comando = new SqlCommand(consulta);
            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);

            return ExecuteNonQuery(comando);
        }

        public List<ProcessedMessage> ObtenerTodo()
        {
            string consulta = @"SELECT MessageId,
                               CorrelationId,
                               RoutingKey,
                               ProcessedAt
                        FROM ProcessedMessages
                        ORDER BY ProcessedAt DESC;";

            SqlCommand comando = new SqlCommand(consulta);

            var result = new List<ProcessedMessage>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(new ProcessedMessage
                {
                    MessageId = reader["MessageId"]?.ToString() ?? string.Empty,
                    CorrelationId = reader["CorrelationId"] == DBNull.Value
                        ? null
                        : reader["CorrelationId"]?.ToString(),
                    RoutingKey = reader["RoutingKey"] == DBNull.Value
                        ? null
                        : reader["RoutingKey"]?.ToString(),
                    ProcessedAt = reader.GetDateTime("ProcessedAt")
                });
            }

            return result;
        }

        public bool Existe(string messageId)
        {
            string consulta = @"SELECT COUNT(1)
                                FROM ProcessedMessages
                                WHERE MessageId = @messageId;";

            SqlCommand comando = new SqlCommand(consulta);
            comando.Parameters.AddWithValue("@messageId", messageId);

            var resultado = ExecuteScalar(comando);

            return Convert.ToInt32(resultado) > 0;
        }
    }
}
