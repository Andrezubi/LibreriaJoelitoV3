using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Interfaces
{
    public interface IProcessedMessageRepositorio
    {
        bool ExisteMessageId(string messageId);

        void RegistrarMensajeProcesado(ProcessedMessage processedMessage);
    }
}