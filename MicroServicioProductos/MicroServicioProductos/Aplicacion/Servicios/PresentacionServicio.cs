
using MicroServicioProductos.Dominio.Modelos;

using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using System.Data;

namespace MicroServicioProductos.Aplicacion.Servicios
{
    public class PresentacionServicio
    {
        private readonly PresentacionRepositorio _presentacionRepo;


        public PresentacionServicio(PresentacionRepositorio presentacionRepo)
        {
            _presentacionRepo = presentacionRepo;
        }

        public List<Presentacion> ObtenerTodo()
        {
            return _presentacionRepo.ObtenerTodo();
        }
    }
}
