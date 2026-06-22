namespace MicroServicioReportes.Dominio.Entidades;

public class TablaReporte
{
    public string Titulo { get; set; } = string.Empty;
    public List<string> Columnas { get; set; } = new();
    public List<Dictionary<string, string>> Filas { get; set; } = new();

    public TablaReporte Clonar()
    {
        return new TablaReporte
        {
            Titulo = Titulo,
            Columnas = Columnas.ToList(),
            Filas = Filas
                .Select(fila => fila.ToDictionary(c => c.Key, c => c.Value))
                .ToList()
        };
    }
}
