using System;

namespace FrontendLibreria.DTOs.Reportes;

public class ReporteRequestDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? IdProducto { get; set; }
    public int? IdCliente { get; set; }
    public int? IdUsuario { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string OrdenPor { get; set; } = "producto";
    public bool Descendente { get; set; }
    public string AgruparPor { get; set; } = "categoria";
}
