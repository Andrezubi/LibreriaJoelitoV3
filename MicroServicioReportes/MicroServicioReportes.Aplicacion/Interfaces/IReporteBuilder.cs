using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Interfaces;

public interface IReporteBuilder
{
    IReporteBuilder UsarPlantilla(DocumentoReporte plantilla);
    IReporteBuilder AgregarEncabezado(string titulo, string subtitulo, string usuario);
    IReporteBuilder AgregarDatosGenerales(IEnumerable<CampoReporte> campos);
    IReporteBuilder AgregarTabla(string titulo, IEnumerable<string> columnas, IEnumerable<IDictionary<string, string>> filas);
    IReporteBuilder AgregarResumen(IEnumerable<CampoReporte> campos);
    IReporteBuilder AgregarGrafico(string titulo, string tipo, IEnumerable<CampoReporte> valores);
    IReporteBuilder AgregarPie(string usuario);
    DocumentoReporte Construir();
}
