using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public class VentaCreadorRepositorio : CreadorRepositorio<Venta>
    {
        public override VentaRepositorio CrearRepositorio()
        {
            return new VentaRepositorio();
        }
    }
}
