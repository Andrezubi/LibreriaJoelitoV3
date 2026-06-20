using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Interfaces;

public interface IPlantillaReporteProveedor
{
    DocumentoReporte ObtenerPlantilla(TipoReporte tipoReporte);
}
