using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;

namespace MicroServicioReportes.Aplicacion.Prototipos;

public class PlantillaReporteProveedor : IPlantillaReporteProveedor
{
    private readonly Dictionary<TipoReporte, DocumentoReporte> _prototipos = new()
    {
        [TipoReporte.ComprobanteVenta] = new DocumentoReporte
        {
            TipoReporte = "Comprobante de Venta",
            Titulo = "Comprobante de Venta",
            Subtitulo = "Documento generado automaticamente al confirmar la venta",
            PiePagina = "Sistema de Gestion de Libreria Joelito"
        },
        [TipoReporte.ListaVentasPorProducto] = new DocumentoReporte
        {
            TipoReporte = "Reporte de Ventas por Producto",
            Titulo = "Ventas por Producto",
            Subtitulo = "Lista ordenada de ventas detalladas por producto",
            PiePagina = "Sistema de Gestion de Libreria Joelito"
        },
        [TipoReporte.ResumenRecaudacion] = new DocumentoReporte
        {
            TipoReporte = "Reporte Sumariado de Recaudacion",
            Titulo = "Resumen de Recaudacion",
            Subtitulo = "Informacion sumariada con grafico estadistico",
            PiePagina = "Sistema de Gestion de Libreria Joelito"
        }
    };

    public DocumentoReporte ObtenerPlantilla(TipoReporte tipoReporte)
    {
        if (!_prototipos.TryGetValue(tipoReporte, out var prototipo))
        {
            throw new InvalidOperationException($"No existe plantilla para el reporte {tipoReporte}.");
        }

        return prototipo.Clonar();
    }
}
