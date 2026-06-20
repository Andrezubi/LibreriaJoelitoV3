using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MicroServicioReportes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteServicio _reporteServicio;

    public ReportesController(IReporteServicio reporteServicio)
    {
        _reporteServicio = reporteServicio;
    }

    [HttpGet("comprobante-venta/{idVenta:int}")]
    public async Task<IActionResult> GenerarComprobanteVenta(
        int idVenta,
        [FromQuery] ReporteRequestDto request,
        CancellationToken cancellationToken)
    {
        PrepararUsuario(request);

        try
        {
            var reporte = await _reporteServicio.GenerarComprobanteVentaAsync(
                idVenta,
                request,
                cancellationToken);

            return File(reporte.Archivo, reporte.ContentType, reporte.NombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("ventas-producto")]
    public async Task<IActionResult> GenerarVentasPorProducto(
        [FromQuery] ReporteRequestDto request,
        CancellationToken cancellationToken)
    {
        PrepararUsuario(request);

        try
        {
            var reporte = await _reporteServicio.GenerarListaVentasPorProductoAsync(
                request,
                cancellationToken);

            return File(reporte.Archivo, reporte.ContentType, reporte.NombreArchivo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("resumen-recaudacion")]
    public async Task<IActionResult> GenerarResumenRecaudacion(
        [FromQuery] ReporteRequestDto request,
        CancellationToken cancellationToken)
    {
        PrepararUsuario(request);

        try
        {
            var reporte = await _reporteServicio.GenerarResumenRecaudacionAsync(
                request,
                cancellationToken);

            return File(reporte.Archivo, reporte.ContentType, reporte.NombreArchivo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private void PrepararUsuario(ReporteRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Usuario))
        {
            return;
        }

        request.Usuario =
            User.FindFirst("NombreCompleto")?.Value ??
            User.FindFirst(ClaimTypes.Name)?.Value ??
            "Sistema";
    }
}
