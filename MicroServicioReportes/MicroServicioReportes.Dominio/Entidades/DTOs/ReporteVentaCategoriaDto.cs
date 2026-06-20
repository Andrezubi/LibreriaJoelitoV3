using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Dominio.Entidades.DTOs
{
    public class ReporteVentaCategoriaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalUnidades { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
