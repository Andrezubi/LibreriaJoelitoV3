using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
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

            MySqlCommand comando = new MySqlCommand(consulta);

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

            MySqlCommand comando = new MySqlCommand(consulta);
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

            MySqlCommand comando = new MySqlCommand(consulta);

            var result = new List<ProcessedMessage>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(new ProcessedMessage
                {
                    MessageId = reader.GetString("MessageId"),
                    CorrelationId = reader.IsDBNull(reader.GetOrdinal("CorrelationId"))
                        ? null
                        : reader.GetString("CorrelationId"),
                    RoutingKey = reader.IsDBNull(reader.GetOrdinal("RoutingKey"))
                        ? null
                        : reader.GetString("RoutingKey"),
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

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@messageId", messageId);

            var resultado = ExecuteScalar(comando);

            return Convert.ToInt32(resultado) > 0;
        }
    }
}