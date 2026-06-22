using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioVentas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaController : ControllerBase
    {
        private readonly FachadaGestionInventario _gestionInventarioServicio;
        private readonly ConsultaVentaServicio _consultaVentaServicio;

        public VentaController(
            FachadaGestionInventario gestionInventarioServicio,
            ConsultaVentaServicio consultaVentaServicio)
        {
            _gestionInventarioServicio = gestionInventarioServicio;
            _consultaVentaServicio = consultaVentaServicio;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_consultaVentaServicio.CargarVentas());
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegistrarVentaRequestDto request)
        {
            if (request == null)
                return BadRequest("La solicitud no puede estar vacía.");

            var resultado = _gestionInventarioServicio.RegistrarVenta(request);

            if (!resultado.IsSuccess)
                return BadRequest(resultado);

            return Accepted(resultado);
        }

        [HttpPut("{idVenta}/anular")]
        public IActionResult AnularVenta(int idVenta, [FromQuery] int idUsuario)
        {
            var resultado = _gestionInventarioServicio.AnularVenta(idVenta, idUsuario);

            if (!resultado.IsSuccess)
                return BadRequest(resultado);

            return Ok(resultado);
        }

        //[HttpGet("{idVenta}/comprobante")]
        //public IActionResult GenerarComprobantePdf(int idVenta)
        //{
        //    var resultado = _consultaVentaServicio.GenerarComprobantePdf(idVenta);

        //    if (!resultado.IsSuccess)
        //        return NotFound(resultado);

        //    return File(
        //        resultado.Value,
        //        "application/pdf",
        //        $"comprobante-venta-{idVenta}.pdf"
        //    );
        //}

        [HttpGet("{idVenta}/completa")]
        public IActionResult ObtenerVentaCompleta(int idVenta)
        {
            var resultado = _consultaVentaServicio.ObtenerVentaCompleta(idVenta);

            if (!resultado.IsSuccess)
                return NotFound(resultado);

            return Ok(resultado.Value);
        }

        //[HttpGet("reporte-servicios")]
        //public IActionResult ObtenerReporteServicios()
        //{
        //    var resultado = _consultaVentaServicio.ObtenerReporteServicios();

        //    if (resultado == null || resultado.Count == 0)
        //        return NoContent();

        //    return Ok(resultado);
        //}
    }
}
