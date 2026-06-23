using FrontendLibreria.Adaptadores.Venta;
using FrontendLibreria.DTOs.VentaDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Ventas
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class IndexVentasModel : PageModel
    {
        private readonly IVentaAdapter _ventaAdapter;

        public List<VentaDTO> Ventas { get; set; } = new();

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public IndexVentasModel(IVentaAdapter ventaAdapter)
        {
            _ventaAdapter = ventaAdapter;
        }

        public async Task OnGetAsync()
        {
            Ventas = await _ventaAdapter.CargarVentasAsync();
        }

        public async Task<JsonResult> OnPostAnularAsync(int idVenta)
        {
            if (idVenta <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Venta inválida."
                });
            }

            VentaCompletaDTO? ventaCompleta = await _ventaAdapter.ObtenerVentaCompletaAsync(idVenta);

            if (ventaCompleta == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró la venta."
                });
            }

            if (ventaCompleta.Venta.EstadoVenta != "CONFIRMADA")
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Solo se puede anular una venta confirmada."
                });
            }

            int idUsuario = ObtenerIdUsuario();

            if (idUsuario <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se pudo identificar al usuario actual."
                });
            }

            ApiResultDTO<int>? resultado = await _ventaAdapter.AnularVentaAsync(idVenta, idUsuario);

            if (resultado != null && resultado.IsSuccess)
            {
                return new JsonResult(new
                {
                    success = true,
                    idVenta = idVenta,
                    message = "La anulación fue solicitada correctamente."
                });
            }

            string mensajeError = resultado?.Error
                ?? resultado?.Errors.FirstOrDefault()
                ?? "Error al anular la venta.";

            return new JsonResult(new
            {
                success = false,
                message = mensajeError
            });
        }

        public async Task<JsonResult> OnGetObtenerDetalleVentaAsync(int idVenta)
        {
            try
            {
                VentaCompletaDTO? resultado = await _ventaAdapter.ObtenerVentaCompletaAsync(idVenta);

                if (resultado == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se encontró la venta."
                    });
                }

                return new JsonResult(new
                {
                    success = true,
                    venta = new
                    {
                        idVenta = resultado.Venta.Id,
                        estado = resultado.Venta.EstadoVenta,
                        textoEstado = ObtenerTextoEstado(resultado.Venta.EstadoVenta),
                        claseEstado = ObtenerClaseEstado(resultado.Venta.EstadoVenta),
                        puedeVerDetalle = PuedeVerDetalle(resultado.Venta.EstadoVenta),
                        puedeAnular = PuedeAnular(resultado.Venta.EstadoVenta),
                        idCliente = resultado.Venta.IdCliente,
                        ciCliente = resultado.Venta.CiCompleto,
                        nombreCliente = resultado.Venta.RazonSocialCliente,
                        emailCliente = resultado.Venta.EmailCliente,
                        clienteFrecuente = resultado.Venta.ClienteFrecuente,
                        fecha = resultado.Venta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        total = resultado.Venta.Total
                    },
                    detalles = resultado.Detalles.Select(detalle => new
                    {
                        producto = detalle.Producto,
                        presentacion = detalle.Presentacion,
                        cantidad = detalle.Cantidad,
                        precioUnitario = detalle.PrecioUnitario,
                        subtotal = detalle.Subtotal
                    }).ToList()
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Error al obtener el detalle de la venta."
                });
            }
        }

        public string ObtenerTextoEstado(string estado)
        {
            return estado switch
            {
                "PENDIENTE" => "Pendiente",
                "STOCK_RESERVADO" => "En proceso",
                "STOCK_RECHAZADO" => "No completada",
                "CONFIRMADA" => "Confirmada",
                "FALLIDA" => "No completada",
                "ANULACION_PENDIENTE" => "Anulación en proceso",
                "ANULADA" => "Anulada",
                _ => "En proceso"
            };
        }

        public string ObtenerClaseEstado(string estado)
        {
            return estado switch
            {
                "PENDIENTE" => "bg-warning text-dark",
                "STOCK_RESERVADO" => "bg-info text-dark",
                "ANULACION_PENDIENTE" => "bg-warning text-dark",
                "CONFIRMADA" => "bg-success",
                "ANULADA" => "bg-secondary",
                "STOCK_RECHAZADO" => "bg-danger",
                "FALLIDA" => "bg-danger",
                _ => "bg-dark"
            };
        }

        public bool PuedeVerDetalle(string estado)
        {
            return estado == "CONFIRMADA" || estado == "ANULADA";
        }

        public bool PuedeAnular(string estado)
        {
            return estado == "CONFIRMADA";
        }

        private int ObtenerIdUsuario()
        {
            var idClaim = User.FindFirst("IdUsuario")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "0";

            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}