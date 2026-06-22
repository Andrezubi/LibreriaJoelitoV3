using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Infraestructura.Persistencia;
using MySql.Data.MySqlClient;

namespace MicroServicioReportes.Infraestructura.Repositorios
{
    public class ProcessedMessageRepositorio : IProcessedMessageRepositorio
    {
        private readonly RepositorioBD _bd;

        public ProcessedMessageRepositorio(RepositorioBD bd)
        {
            _bd = bd;
        }

        public bool ExisteMessageId(string messageId)
        {
            var comando = new MySqlCommand(@"
                SELECT COUNT(1)
                FROM processed_messages
                WHERE message_id = @messageId;
            ");

            comando.Parameters.AddWithValue("@messageId", messageId);

            object? resultado = _bd.ExecuteScalar(comando);

            return Convert.ToInt32(resultado) > 0;
        }

        public void RegistrarMensajeProcesado(ProcessedMessage processedMessage)
        {
            var comando = new MySqlCommand(@"
                INSERT INTO processed_messages (
                    message_id,
                    correlation_id,
                    routing_key,
                    processed_at
                )
                VALUES (
                    @messageId,
                    @correlationId,
                    @routingKey,
                    @processedAt
                );
            ");

            comando.Parameters.AddWithValue("@messageId", processedMessage.MessageId);
            comando.Parameters.AddWithValue("@correlationId", processedMessage.CorrelationId);
            comando.Parameters.AddWithValue("@routingKey", processedMessage.RoutingKey);
            comando.Parameters.AddWithValue("@processedAt", processedMessage.ProcessedAt);

            _bd.ExecuteNonQuery(comando);
        }
    }
}