using MicroServicioReportes.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Aplicacion.Factoria
{
    public class ExcelCreador : CreadorAbstracto
    {
        public override IGeneradorReporte CrearGenerador() => new ExcelGenerador();
    }
}
