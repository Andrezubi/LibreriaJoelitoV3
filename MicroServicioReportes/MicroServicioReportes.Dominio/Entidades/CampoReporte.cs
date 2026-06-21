namespace MicroServicioReportes.Dominio.Entidades;

public class CampoReporte
{
    public string Etiqueta { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;

    public CampoReporte Clonar()
    {
        return new CampoReporte
        {
            Etiqueta = Etiqueta,
            Valor = Valor
        };
    }
}
