using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Builders;

public class ReporteDocumentoBuilder : IReporteBuilder
{
    private DocumentoReporte _documento = new();

    public IReporteBuilder UsarPlantilla(DocumentoReporte plantilla)
    {
        _documento = plantilla.Clonar();
        _documento.FechaGeneracion = DateTime.Now;
        return this;
    }

    public IReporteBuilder AgregarEncabezado(string titulo, string subtitulo, string usuario)
    {
        _documento.Titulo = titulo;
        _documento.Subtitulo = subtitulo;
        _documento.UsuarioGenerador = usuario;
        return this;
    }

    public IReporteBuilder AgregarDatosGenerales(IEnumerable<CampoReporte> campos)
    {
        _documento.DatosGenerales.AddRange(campos.Select(c => c.Clonar()));
        return this;
    }

    public IReporteBuilder AgregarTabla(
        string titulo,
        IEnumerable<string> columnas,
        IEnumerable<IDictionary<string, string>> filas)
    {
        _documento.Tablas.Add(new TablaReporte
        {
            Titulo = titulo,
            Columnas = columnas.ToList(),
            Filas = filas
                .Select(fila => fila.ToDictionary(c => c.Key, c => c.Value))
                .ToList()
        });

        return this;
    }

    public IReporteBuilder AgregarResumen(IEnumerable<CampoReporte> campos)
    {
        _documento.Resumen.AddRange(campos.Select(c => c.Clonar()));
        return this;
    }

    public IReporteBuilder AgregarGrafico(string titulo, string tipo, IEnumerable<CampoReporte> valores)
    {
        _documento.Graficos.Add(new GraficoReporte
        {
            Titulo = titulo,
            Tipo = tipo,
            Valores = valores.Select(v => v.Clonar()).ToList()
        });

        return this;
    }

    public IReporteBuilder AgregarPie(string usuario)
    {
        _documento.PiePagina =
            $"Generado el {_documento.FechaGeneracion:dd/MM/yyyy HH:mm} por {usuario}";
        return this;
    }

    public DocumentoReporte Construir()
    {
        return _documento.Clonar();
    }
}
