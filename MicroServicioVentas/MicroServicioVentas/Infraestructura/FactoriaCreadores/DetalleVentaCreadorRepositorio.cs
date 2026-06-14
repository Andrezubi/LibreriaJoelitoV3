using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;

namespace MicroServicioVentas.Infraestructura.FactoriaCreadores
{
    public class DetalleVentaCreadorRepositorio : CreadorRepositorio<DetalleVenta>
    {
        public override DetalleVentaRepositorio CrearRepositorio()
        {
            return new DetalleVentaRepositorio();
        }
    }
}
