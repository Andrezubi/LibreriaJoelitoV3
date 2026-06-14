using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using MicroServicioVentas.Aplicacion.DTOs.ServicioVentaDTOs;
using MicroServicioVentas.Aplicacion.Servicios;

namespace MicroServicioVentas.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VentaController : ControllerBase
    {
        GestionInventarioServicio _gestionInventarioServicio;
        ConsultaVentaServicio _consultaVentaServicio;
        public VentaController(GestionInventarioServicio gestionInventarioServicio, ConsultaVentaServicio consultaVentaServicio)
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

            if (request.Venta == null)
                return BadRequest("La venta es obligatoria.");

            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest("La venta debe tener al menos un producto.");

            var resultado = _gestionInventarioServicio.RegistrarVenta(
                request.Venta,
                request.Detalles
            );

            if (!resultado.IsSuccess)
                return BadRequest(resultado);

            return Ok(resultado);
        }

        [HttpPut("{idVenta}/anular")]
        public IActionResult AnularVenta(int idVenta, [FromQuery] int idEmpleado)
        {
            var resultado = _gestionInventarioServicio.AnularVenta(idVenta, idEmpleado);

            if (!resultado.IsSuccess)
                return BadRequest(resultado);

            return Ok(resultado);
        }


        [HttpGet("presentaciones")]
        public IActionResult GetPresentacionProductosByFrase([FromQuery] string frase)
        {

            var resultado = _consultaVentaServicio.getPresentacionProductosByFrase(frase);
            return Ok(resultado);
        }



        [HttpGet("productos/{idProducto}/presentaciones/{idPresentacion}")]
        public IActionResult GetPresentacionProductoByIds(int idProducto, int idPresentacion)
        {
            var resultado = _consultaVentaServicio.GetPresentacionProductoByIds(idProducto, idPresentacion);

            if (!resultado.IsSuccess)
                return NotFound(resultado);

            return Ok(resultado.Value);
        }


        [HttpGet("{idVenta}/comprobante")]
        public IActionResult GenerarComprobantePdf(int idVenta)
        {
            var resultado = _consultaVentaServicio.GenerarComprobantePdf(idVenta);

            if (!resultado.IsSuccess)
                return NotFound(resultado);

            return File(
                resultado.Value,
                "application/pdf",
                $"comprobante-venta-{idVenta}.pdf"
            );
        }


        [HttpGet("{idVenta}/completa")]
        public IActionResult ObtenerVentaCompleta(int idVenta)
        {
            var resultado = _consultaVentaServicio.ObtenerVentaCompleta(idVenta);

            if (!resultado.IsSuccess)
                return NotFound(resultado);

            return Ok(resultado.Value);
        }

        [HttpGet("reporte-servicios")]
        public IActionResult ObtenerReporteServicios()
        {
            var resultado = _consultaVentaServicio.ObtenerReporteServicios();

            if (resultado == null)
                return NoContent();

            return Ok(resultado);
        }
    }
}
