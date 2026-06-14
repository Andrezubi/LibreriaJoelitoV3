using FrontendLibreria.Adapters.Cliente;
using FrontendLibreria.Adapters.Venta;
using FrontendLibreria.DTOs;
using FrontendLibreria.DTOs.VentaDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Ventas
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class RegistrarModel : PageModel
    {
        private readonly IVentaAdapter _ventaAdapter;
        private readonly IAdaptadorCliente _clienteAdapter;

        public RegistrarModel(
            IVentaAdapter ventaAdapter,
            IAdaptadorCliente clienteAdapter)
        {
            _ventaAdapter = ventaAdapter;
            _clienteAdapter = clienteAdapter;
        }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetBuscarClienteAsync(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
                return new JsonResult(new { success = false, message = "CI no proporcionado" });

            ClienteDto? cliente = await _clienteAdapter.ObtenerPorCiAsync(ci);

            if (cliente == null)
                return new JsonResult(new { success = false, message = "Cliente no encontrado" });

            return new JsonResult(new
            {
                success = true,
                cliente = new
                {
                    cliente.Id,
                    cliente.RazonSocial,
                    cliente.Ci,
                    cliente.Complemento
                }
            });
        }

        public async Task<JsonResult> OnGetBuscarClientesParcialAsync(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
            {
                return new JsonResult(new
                {
                    success = false,
                    clientes = new List<object>()
                });
            }

            List<ClienteDto> clientes = await _clienteAdapter.ObtenerSimilaresPorCiAsync(ci);

            var lista = clientes.Select(cliente => new
            {
                id = cliente.Id,
                razonSocial = cliente.RazonSocial,
                ci = cliente.Ci,
                complemento = cliente.Complemento,
                ciCompleto = cliente.CiCompleto
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                clientes = lista
            });
        }

        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> OnPostCrearClienteAsync([FromBody] ClienteDto cliente)
        {
            if (cliente == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Datos inválidos"
                });
            }


            var idUsuario = ObtenerIdUsuario(); // Reemplaza con el ID del usuario actual
            cliente.Estado = true;
            cliente.FechaRegistro = DateTime.Now;
            cliente.IdUsuario = idUsuario;

            ResultadoApi resultado = await _clienteAdapter.InsertarAsync(cliente);

            if (!resultado.Success)
            {
                string mensaje = resultado.Errors.Any()
                    ? string.Join("\n", resultado.Errors)
                    : "Error al crear cliente.";

                return new JsonResult(new
                {
                    success = false,
                    message = mensaje
                });
            }

            ClienteDto? nuevo = await _clienteAdapter.ObtenerPorCiAsync(cliente.Ci);

            if (nuevo == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Cliente creado, pero no se pudo recuperar desde la API."
                });
            }

            return new JsonResult(new
            {
                success = true,
                cliente = new
                {
                    nuevo.Id,
                    nuevo.RazonSocial,
                    nuevo.Ci,
                    nuevo.Complemento
                }
            });
        }

        public async Task<JsonResult> OnGetBuscarNombreAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return new JsonResult(new List<object>());

            List<PresentacionProductoVentaDTO> productos =
                await _ventaAdapter.ObtenerPresentacionesPorFraseAsync(termino);

            var listaNombres = productos.Select(producto => new
            {
                texto = producto.Descripcion,
                idProducto = producto.IdProducto,
                idPresentacion = producto.IdPresentacion
            }).ToList();

            return new JsonResult(listaNombres);
        }

        public async Task<IActionResult> OnGetObtenerDetalleProductoAsync(
            string frase,
            int idProducto,
            int idPresentacion)
        {
            if (string.IsNullOrEmpty(frase))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El nombre está vacío."
                });
            }

            PresentacionProductoVentaDTO? producto =
                await _ventaAdapter.ObtenerPresentacionProductoByIdsAsync(idProducto, idPresentacion);

            if (producto == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró el producto."
                });
            }

            return new JsonResult(new
            {
                success = true,
                producto = new
                {
                    idProducto = producto.IdProducto,
                    idPresentacion = producto.IdPresentacion,
                    nombre = !string.IsNullOrWhiteSpace(producto.Nombre)
                        ? producto.Nombre
                        : producto.Descripcion,
                    precioUnitario = producto.PrecioUnitario > 0
                        ? producto.PrecioUnitario
                        : producto.Precio
                }
            });
        }

        public async Task<IActionResult> OnGetImprimirComprobanteAsync(int idVenta)
        {
            if (idVenta <= 0)
                return BadRequest("ID de venta inválido.");

            try
            {
                byte[] pdf = await _ventaAdapter.GenerarComprobantePdfAsync(idVenta);

                if (pdf == null || pdf.Length == 0)
                    return Content("Error: no se pudo generar el comprobante.");

                string nombreArchivo = $"Comprobante_Venta_{idVenta}.pdf";

                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = nombreArchivo,
                    Inline = true
                };

                Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        public class RegistrarVentaDto
        {
            public int IdCliente { get; set; }
            public List<DetalleVentaDTO> Detalles { get; set; } = new List<DetalleVentaDTO>();
        }

        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> OnPostRegistrarVentaAsync([FromBody] RegistrarVentaDto dto)
        {
            if (dto == null || dto.Detalles == null || !dto.Detalles.Any())
                return new JsonResult(new { success = false, message = "La venta no tiene productos." });

            if (dto.IdCliente <= 0)
                return new JsonResult(new { success = false, message = "Cliente no válido." });


            var idUsuario = ObtenerIdUsuario(); // Reemplaza con el ID del usuario actual
            decimal total = dto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            var request = new RegistrarVentaRequestDTO
            {
                Venta = new VentaRegistroDTO
                {
                    IdCliente = dto.IdCliente,
                    IdUsuario = idUsuario,
                    Fecha = DateTime.Now,
                    Total = total,
                    Estado = true
                },
                Detalles = dto.Detalles
            };

            ApiResultDTO<int>? result = await _ventaAdapter.RegistrarVentaAsync(request);

            if (result != null && result.IsSuccess)
            {
                return new JsonResult(new
                {
                    success = true,
                    idVenta = result.Value,
                    message = "Venta registrada correctamente."
                });
            }

            string mensajeError = result?.Error
                ?? result?.Errors.FirstOrDefault()
                ?? "Error al registrar.";

            return new JsonResult(new
            {
                success = false,
                message = mensajeError
            });
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