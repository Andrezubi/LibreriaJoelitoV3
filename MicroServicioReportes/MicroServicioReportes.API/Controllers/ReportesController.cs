using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Infraestructura.Generadores;
using MicroServicioReportes.Infraestructura.Repositorios;
using MicroServicioReportes.Dominio.Interfaces;
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
    private readonly IBitacoraReporteRepositorio _bitacoraRepositorio;

    public ReportesController(IReporteServicio reporteServicio, IComprobanteVentaRepositorio comprobanteVentaRepositorio, IComprobanteVentaPdfServicio comprobanteVentaPdfServicio, IBitacoraReporteRepositorio bitacoraRepositorio)
    {
        _reporteServicio = reporteServicio;
        _comprobanteVentaRepositorio = comprobanteVentaRepositorio;
        _comprobanteVentaPdfServicio = comprobanteVentaPdfServicio;
        _bitacoraRepositorio = bitacoraRepositorio;
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

    [HttpGet("ventas-producto/datos")]
    public async Task<IActionResult> ObtenerDatosVentasPorProducto(
        [FromQuery] ReporteRequestDto request,
        CancellationToken cancellationToken)
    {
        PrepararUsuario(request);

        try
        {
            var reporte = await _reporteServicio.ObtenerDatosVentasPorProductoAsync(
                request,
                cancellationToken);

            return Ok(reporte);
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

    [HttpGet("bitacora")]
    public async Task<IActionResult> ObtenerBitacora(CancellationToken cancellationToken)
    {
        var entradas = await _bitacoraRepositorio.ObtenerTodoAsync(cancellationToken);
        return Ok(entradas);
    }

    private void PrepararUsuario(ReporteRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Usuario))
        {
            PrepararIdUsuario(request);
            return;
        }

        request.Usuario =
            User.FindFirst("NombreCompleto")?.Value ??
            User.FindFirst(ClaimTypes.Name)?.Value ??
            "Sistema";

        PrepararIdUsuario(request);
    }

    private void PrepararIdUsuario(ReporteRequestDto request)
    {
        if (request.IdUsuario.HasValue)
        {
            return;
        }

        var idUsuario =
            User.FindFirst("IdUsuario")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(idUsuario, out var idDesdeToken))
        {
            request.IdUsuario = idDesdeToken;
            return;
        }

        if (Request.Headers.TryGetValue("X-IdUsuario", out var idDesdeHeader) &&
            int.TryParse(idDesdeHeader, out var idHeader))
        {
            request.IdUsuario = idHeader;
        }
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
