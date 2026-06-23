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

        public async Task<IActionResult> OnPostAnularAsync(int idVenta)
        {
            if (idVenta <= 0)
            {
                MensajeError = "ID de venta inválido.";
                return RedirectToPage();
            }

            var ventaCompleta = await _ventaAdapter.ObtenerVentaCompletaAsync(idVenta);

            if (ventaCompleta == null)
            {
                MensajeError = "No se encontró la venta.";
                return RedirectToPage();
            }

            if (ventaCompleta.Venta.EstadoVenta != "CONFIRMADA")
            {
                MensajeError = "Solo se puede anular una venta confirmada.";
                return RedirectToPage();
            }

            int idUsuario = ObtenerIdUsuario();

            var resultado = await _ventaAdapter.AnularVentaAsync(idVenta, idUsuario);

            if (resultado != null && resultado.IsSuccess)
            {
                MensajeExito = $"La venta #{idVenta} inició el proceso de anulación correctamente.";
            }
            else
            {
                string mensajeError = resultado?.Error
                    ?? resultado?.Errors.FirstOrDefault()
                    ?? "Error al anular la venta.";

                MensajeError = mensajeError;
            }

            return RedirectToPage();
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
                        correlationId = resultado.Venta.CorrelationId,
                        estado = resultado.Venta.EstadoVenta,
                        idCliente = resultado.Venta.IdCliente,
                        ciCliente = resultado.Venta.CiCompleto,
                        nombreCliente = resultado.Venta.RazonSocialCliente,
                        emailCliente = resultado.Venta.EmailCliente,
                        clienteFrecuente = resultado.Venta.ClienteFrecuente,
                        fecha = resultado.Venta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        usuario = $"Usuario {resultado.Venta.IdUsuario}",
                        total = resultado.Venta.Total
                    },
                    detalles = resultado.Detalles.Select(detalle => new
                    {
                        producto = detalle.Producto,
                        presentacion = detalle.Presentacion,
                        cantidad = detalle.Cantidad,
                        precioUnitario = detalle.PrecioUnitario,
                        subtotal = detalle.Subtotal,
                        estado = detalle.Estado
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public string ObtenerTextoEstado(string estado)
        {
            return estado switch
            {
                "PENDIENTE" => "Pendiente",
                "STOCK_RESERVADO" => "Stock reservado",
                "STOCK_RECHAZADO" => "Stock rechazado",
                "CONFIRMADA" => "Confirmada",
                "FALLIDA" => "Fallida",
                "ANULACION_PENDIENTE" => "Anulación pendiente",
                "ANULADA" => "Anulada",
                _ => estado
            };
        }

        public string ObtenerClaseEstado(string estado)
        {
            return estado switch
            {
                "PENDIENTE" => "bg-warning text-dark",
                "ANULACION_PENDIENTE" => "bg-warning text-dark",
                "CONFIRMADA" => "bg-success",
                "ANULADA" => "bg-secondary",
                "STOCK_RECHAZADO" => "bg-danger",
                "FALLIDA" => "bg-danger",
                _ => "bg-dark"
            };
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