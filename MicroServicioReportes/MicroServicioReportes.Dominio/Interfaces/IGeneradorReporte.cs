using MicroServicioReportes.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Dominio.Interfaces
{
    public interface IGeneradorReporte
    {
        string ContentType { get; }
        string Extension { get; }
        byte[] Generar(DocumentoReporte documento);
    }
}
