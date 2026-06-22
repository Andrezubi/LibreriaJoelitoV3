using MicroServicioReportes.Dominio.Prototipos;

namespace MicroServicioReportes.Dominio.Entidades;

public class DocumentoReporte : IPrototipo<DocumentoReporte>
{
    public string Titulo { get; set; } = string.Empty;
    public string Subtitulo { get; set; } = string.Empty;
    public string LogoTexto { get; set; } = "LIBRERIA JOELITO";
    public string ColorPrimarioHex { get; set; } = "#7B2CBF";
    public string TipoReporte { get; set; } = string.Empty;
    public string UsuarioGenerador { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    public string EstadoDocumento { get; set; } = "Generado";
    public List<CampoReporte> DatosGenerales { get; set; } = new();
    public List<TablaReporte> Tablas { get; set; } = new();
    public List<CampoReporte> Resumen { get; set; } = new();
    public List<GraficoReporte> Graficos { get; set; } = new();
    public string PiePagina { get; set; } = string.Empty;

    public DocumentoReporte Clonar()
    {
        return new DocumentoReporte
        {
            Titulo = Titulo,
            Subtitulo = Subtitulo,
            LogoTexto = LogoTexto,
            ColorPrimarioHex = ColorPrimarioHex,
            TipoReporte = TipoReporte,
            UsuarioGenerador = UsuarioGenerador,
            FechaGeneracion = FechaGeneracion,
            EstadoDocumento = EstadoDocumento,
            DatosGenerales = DatosGenerales.Select(c => c.Clonar()).ToList(),
            Tablas = Tablas.Select(t => t.Clonar()).ToList(),
            Resumen = Resumen.Select(c => c.Clonar()).ToList(),
            Graficos = Graficos.Select(g => g.Clonar()).ToList(),
            PiePagina = PiePagina
        };
    }
}
