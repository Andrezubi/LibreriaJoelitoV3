using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MicroServicioProductos.Dominio.Modelos;

using System.Data;
using Microsoft.Data.SqlClient;


namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
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

            SqlCommand comando = new SqlCommand(consulta);

            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);
            comando.Parameters.AddWithValue("@correlationId", mensaje.CorrelationId);
            comando.Parameters.AddWithValue("@exchangeName", mensaje.ExchangeName);
            comando.Parameters.AddWithValue("@routingKey", mensaje.RoutingKey);
            comando.Parameters.AddWithValue("@messageType", mensaje.MessageType);
            comando.Parameters.AddWithValue("@payload", mensaje.Payload);
            comando.Parameters.AddWithValue("@status", mensaje.Status);
            comando.Parameters.AddWithValue("@retryCount", mensaje.RetryCount);
            comando.Parameters.AddWithValue("@lastError", (object?)mensaje.LastError ?? DBNull.Value);

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

            SqlCommand comando = new SqlCommand(consulta);

            comando.Parameters.AddWithValue("@status", mensaje.Status);
            comando.Parameters.AddWithValue("@retryCount", mensaje.RetryCount);
            comando.Parameters.AddWithValue("@lastError", (object?)mensaje.LastError ?? DBNull.Value);
            comando.Parameters.AddWithValue("@publishedAt", (object?)mensaje.PublishedAt ?? DBNull.Value);
            comando.Parameters.AddWithValue("@lastAttemptAt", (object?)mensaje.LastAttemptAt ?? DBNull.Value);
            comando.Parameters.AddWithValue("@messageId", mensaje.MessageId);

            return ExecuteNonQuery(comando);
        }

        public int Eliminar(OutboxMessage mensaje)
        {
            string consulta = @"DELETE FROM OutboxMessages
                                WHERE MessageId = @messageId;";

            SqlCommand comando = new SqlCommand(consulta);
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

            SqlCommand comando = new SqlCommand(consulta);

            var result = new List<OutboxMessage>();

            using var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(MapearOutboxMessage(reader));
            }

            return result;
        }

        public List<OutboxMessage> ObtenerPendientes(int limite = 20)
        {
            // SQL Server: usar TOP (@limite). TOP acepta parámetro en paréntesis en versiones modernas.
            string consulta = $@"SELECT TOP (@limite) Id,
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
                        ORDER BY CreatedAt ASC;";

            SqlCommand comando = new SqlCommand(consulta);

            comando.Parameters.AddWithValue("@pending", EstadosOutboxMessage.Pending);
            comando.Parameters.AddWithValue("@failed", EstadosOutboxMessage.Failed);
            comando.Parameters.AddWithValue("@limite", limite);

            var result = new List<OutboxMessage>();

            using var reader = ExecuteReader(comando);

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

            SqlCommand comando = new SqlCommand(consulta);

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

            SqlCommand comando = new SqlCommand(consulta);

            comando.Parameters.AddWithValue("@status", EstadosOutboxMessage.Failed);
            comando.Parameters.AddWithValue("@error", error);
            comando.Parameters.AddWithValue("@messageId", messageId);

            return ExecuteNonQuery(comando);
        }

        private OutboxMessage MapearOutboxMessage(SqlDataReader reader)
        {
            return new OutboxMessage
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),

                MessageId = ObtenerString(reader, "MessageId"),
                CorrelationId = ObtenerString(reader, "CorrelationId"),
                ExchangeName = ObtenerString(reader, "ExchangeName"),
                RoutingKey = ObtenerString(reader, "RoutingKey"),
                MessageType = ObtenerString(reader, "MessageType"),
                Payload = ObtenerString(reader, "Payload"),
                Status = ObtenerString(reader, "Status"),

                RetryCount = reader.GetInt32(reader.GetOrdinal("RetryCount")),

                LastError = ObtenerStringNullable(reader, "LastError"),

                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                PublishedAt = reader.IsDBNull(reader.GetOrdinal("PublishedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("PublishedAt")),

                LastAttemptAt = reader.IsDBNull(reader.GetOrdinal("LastAttemptAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("LastAttemptAt")),

                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private string ObtenerString(SqlDataReader reader, string columna)
        {
            var valor = reader[columna];

            if (valor == null || valor == DBNull.Value)
                return string.Empty;

            return valor.ToString() ?? string.Empty;
        }

        private string? ObtenerStringNullable(SqlDataReader reader, string columna)
        {
            var valor = reader[columna];

            if (valor == null || valor == DBNull.Value)
                return null;

            return valor.ToString();
        }
    }
}