namespace MicroServicioReportes.Aplicacion.Interfaces
{
    public interface IUnidadTrabajo
    {
        void BeginTransaction();

        void Commit();

        void Rollback();
    }
}