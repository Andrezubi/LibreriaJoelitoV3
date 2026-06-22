using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Infraestructura.Generadores;
using MicroServicioReportes.Infraestructura.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MicroServicioReportes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteServicio _reporteServicio;
    private readonly IComprobanteVentaRepositorio _comprobanteVentaRepositorio;
    private readonly IComprobanteVentaPdfServicio _comprobanteVentaPdfServicio;

    public ReportesController(IReporteServicio reporteServicio, IComprobanteVentaRepositorio comprobanteVentaRepositorio, IComprobanteVentaPdfServicio comprobanteVentaPdfServicio)
    {
        _reporteServicio = reporteServicio;
        _comprobanteVentaRepositorio = comprobanteVentaRepositorio;
        _comprobanteVentaPdfServicio = comprobanteVentaPdfServicio;
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

    [HttpGet("comprobante-venta/{idVenta:int}/ver")]
    public IActionResult VerComprobanteVenta(int idVenta)
    {
        var comprobante = _comprobanteVentaRepositorio.ObtenerPorVentaId(idVenta);

        if (comprobante == null)
        {
            return NotFound(new
            {
                error = $"No existe comprobante generado para la venta {idVenta}."
            });
        }

        var pdf = _comprobanteVentaPdfServicio.GenerarComprobanteVenta(comprobante);

        if (pdf.Length == 0)
        {
            return BadRequest(new
            {
                error = "No se pudo generar el PDF del comprobante."
            });
        }

        Response.Headers["Content-Disposition"] =
            $"inline; filename=comprobante-{comprobante.NumeroComprobante}.pdf";

        return File(pdf, "application/pdf");
    }
}
