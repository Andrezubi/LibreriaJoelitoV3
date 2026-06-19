using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Dominio.Modelos.Enum;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class OutboxMessageRepositorio : RepositorioBD, IRepositorio<OutboxMessage>
    {
        public int Insertar(OutboxMessage mensaje)
        {
            string consulta = @"INSERT INTO OutboxMessages (
                                    MessageId,
                                    CorrelationId,
                                    ExchangeName,
                                    RoutingKey,
                                    MessageType,
                                    Payload,
                                    Status,
                                    RetryCount,
                                    LastError
                                )
                                VALUES (
                                    @messageId,
                                    @correlationId,
                                    @exchangeName,
                                    @routingKey,
                                    @messageType,
                                    @payload,
                                    @status,
                                    @retryCount,
                                    @lastError
                                );";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);
            comando.Parameters.AddWithValue("@correlationId", mensaje.CorrelationId);
            comando.Parameters.AddWithValue("@exchangeName", mensaje.ExchangeName);
            comando.Parameters.AddWithValue("@routingKey", mensaje.RoutingKey);
            comando.Parameters.AddWithValue("@messageType", mensaje.MessageType);
            comando.Parameters.AddWithValue("@payload", mensaje.Payload);
            comando.Parameters.AddWithValue("@status", mensaje.Status);
            comando.Parameters.AddWithValue("@retryCount", mensaje.RetryCount);
            comando.Parameters.AddWithValue("@lastError", mensaje.LastError);

            return ExecuteNonQuery(comando);
        }

        public int Actualizar(OutboxMessage mensaje)
        {
            string consulta = @"UPDATE OutboxMessages
                                SET Status = @status,
                                    RetryCount = @retryCount,
                                    LastError = @lastError,
                                    PublishedAt = @publishedAt,
                                    LastAttemptAt = @lastAttemptAt
                                WHERE MessageId = @messageId;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@status", mensaje.Status);
            comando.Parameters.AddWithValue("@retryCount", mensaje.RetryCount);
            comando.Parameters.AddWithValue("@lastError", mensaje.LastError);
            comando.Parameters.AddWithValue("@publishedAt", mensaje.PublishedAt);
            comando.Parameters.AddWithValue("@lastAttemptAt", mensaje.LastAttemptAt);
            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);

            return ExecuteNonQuery(comando);
        }

        public int Eliminar(OutboxMessage mensaje)
        {
            string consulta = @"DELETE FROM OutboxMessages
                                WHERE MessageId = @messageId;";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);

            return ExecuteNonQuery(comando);
        }

        public List<OutboxMessage> ObtenerTodo()
        {
            string consulta = @"SELECT Id,
                                       MessageId,
                                       CorrelationId,
                                       ExchangeName,
                                       RoutingKey,
                                       MessageType,
                                       Payload,
                                       Status,
                                       RetryCount,
                                       LastError,
                                       CreatedAt,
                                       PublishedAt,
                                       LastAttemptAt,
                                       UpdatedAt
                                FROM OutboxMessages
                                ORDER BY CreatedAt DESC;";

            MySqlCommand comando = new MySqlCommand(consulta);

            var result = new List<OutboxMessage>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(MapearOutboxMessage(reader));
            }

            return result;
        }

        public List<OutboxMessage> ObtenerPendientes(int limite = 20)
        {
            string consulta = @"SELECT Id,
                                       MessageId,
                                       CorrelationId,
                                       ExchangeName,
                                       RoutingKey,
                                       MessageType,
                                       Payload,
                                       Status,
                                       RetryCount,
                                       LastError,
                                       CreatedAt,
                                       PublishedAt,
                                       LastAttemptAt,
                                       UpdatedAt
                                FROM OutboxMessages
                                WHERE Status IN (@pending, @failed)
                                ORDER BY CreatedAt ASC
                                LIMIT @limite;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@pending", EstadosOutboxMessage.Pending);
            comando.Parameters.AddWithValue("@failed", EstadosOutboxMessage.Failed);
            comando.Parameters.AddWithValue("@limite", limite);

            var result = new List<OutboxMessage>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(MapearOutboxMessage(reader));
            }

            return result;
        }

        public int MarcarComoPublicado(string messageId)
        {
            string consulta = @"UPDATE OutboxMessages
                                SET Status = @status,
                                    PublishedAt = CURRENT_TIMESTAMP,
                                    LastAttemptAt = CURRENT_TIMESTAMP,
                                    LastError = NULL
                                WHERE MessageId = @messageId;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@status", EstadosOutboxMessage.Published);
            comando.Parameters.AddWithValue("@messageId", messageId);

            return ExecuteNonQuery(comando);
        }

        public int MarcarComoFallido(string messageId, string error)
        {
            string consulta = @"UPDATE OutboxMessages
                                SET Status = @status,
                                    RetryCount = RetryCount + 1,
                                    LastError = @error,
                                    LastAttemptAt = CURRENT_TIMESTAMP
                                WHERE MessageId = @messageId;";

            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@status", EstadosOutboxMessage.Failed);
            comando.Parameters.AddWithValue("@error", error);
            comando.Parameters.AddWithValue("@messageId", messageId);

            return ExecuteNonQuery(comando);
        }

        private OutboxMessage MapearOutboxMessage(MySqlDataReader reader)
        {
            return new OutboxMessage
            {
                Id = reader.GetInt64("Id"),
                MessageId = reader.GetString("MessageId"),
                CorrelationId = reader.GetString("CorrelationId"),
                ExchangeName = reader.GetString("ExchangeName"),
                RoutingKey = reader.GetString("RoutingKey"),
                MessageType = reader.GetString("MessageType"),
                Payload = reader.GetString("Payload"),
                Status = reader.GetString("Status"),
                RetryCount = reader.GetInt32("RetryCount"),
                LastError = reader.IsDBNull(reader.GetOrdinal("LastError"))
                    ? null
                    : reader.GetString("LastError"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                PublishedAt = reader.IsDBNull(reader.GetOrdinal("PublishedAt"))
                    ? null
                    : reader.GetDateTime("PublishedAt"),
                LastAttemptAt = reader.IsDBNull(reader.GetOrdinal("LastAttemptAt"))
                    ? null
                    : reader.GetDateTime("LastAttemptAt"),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime("UpdatedAt")
            };
        }
    }
}