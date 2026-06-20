using MicroServicioReportes.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Aplicacion.Factoria
{
    public class PdfGenerador : CreadorAbstracto
    {
        public override IGeneradorReporte CrearGenerador() => new PdfGenerador();
       
    }
}
