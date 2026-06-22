namespace MicroServicioReportes.Dominio.Entidades;

public class GraficoReporte
{
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Barras";
    public List<CampoReporte> Valores { get; set; } = new();

    public GraficoReporte Clonar()
    {
        return new GraficoReporte
        {
            Titulo = Titulo,
            Tipo = Tipo,
            Valores = Valores.Select(v => v.Clonar()).ToList()
        };
    }
}
