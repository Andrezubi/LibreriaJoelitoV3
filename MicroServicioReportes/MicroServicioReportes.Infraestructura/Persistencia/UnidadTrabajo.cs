using MicroServicioReportes.Aplicacion.Interfaces;

namespace MicroServicioReportes.Infraestructura.Persistencia
{
    public class UnidadTrabajo : IUnidadTrabajo
    {
        private readonly RepositorioBD _repositorioBD;

        public UnidadTrabajo(RepositorioBD repositorioBD)
        {
            _repositorioBD = repositorioBD;
        }

        public void BeginTransaction()
        {
            _repositorioBD.BeginTransaction();
        }

        public void Commit()
        {
            _repositorioBD.Commit();
        }

        public void Rollback()
        {
            _repositorioBD.Rollback();
        }
    }
}