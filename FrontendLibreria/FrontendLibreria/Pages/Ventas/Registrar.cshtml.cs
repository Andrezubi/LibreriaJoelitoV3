using FrontendLibreria.Adaptadores.Cliente;
using FrontendLibreria.Adaptadores.Producto;
using FrontendLibreria.Adaptadores.Venta;
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
        private readonly IAdaptadorProducto _productoAdapter;

        public RegistrarModel(
            IVentaAdapter ventaAdapter,
            IAdaptadorCliente clienteAdapter,
            IAdaptadorProducto productoAdapter)
        {
            _ventaAdapter = ventaAdapter;
            _clienteAdapter = clienteAdapter;
            _productoAdapter = productoAdapter;
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
                    id = cliente.Id,
                    razonSocial = cliente.RazonSocial,
                    ci = cliente.Ci,
                    complemento = cliente.Complemento,
                    email = cliente.Email,
                    clienteFrecuente = cliente.ClienteFrecuente,
                    ciCompleto = cliente.CiCompleto
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
                email = cliente.Email,
                clienteFrecuente = cliente.ClienteFrecuente,
                ciCompleto = cliente.CiCompleto
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                clientes = lista
            });
        }

        public async Task<JsonResult> OnPostCrearClienteAsync([FromBody] ClienteDto cliente)
        {
            if (cliente == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Datos inválidos."
                });
            }

            if (string.IsNullOrWhiteSpace(cliente.RazonSocial))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "La razón social es obligatoria."
                });
            }

            if (string.IsNullOrWhiteSpace(cliente.Ci))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El CI es obligatorio."
                });
            }

            int idUsuario = ObtenerIdUsuario();

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
                    id = nuevo.Id,
                    razonSocial = nuevo.RazonSocial,
                    ci = nuevo.Ci,
                    complemento = nuevo.Complemento,
                    email = nuevo.Email,
                    clienteFrecuente = nuevo.ClienteFrecuente,
                    ciCompleto = nuevo.CiCompleto
                }
            });
        }

        public async Task<JsonResult> OnGetBuscarNombreAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return new JsonResult(new List<object>());

            List<PresentacionProductoVentaDTO> productos =
                await _productoAdapter.ObtenerPresentacionesPorFraseAsync(termino);

            var listaNombres = productos.Select(producto => new
            {
                texto = !string.IsNullOrWhiteSpace(producto.Descripcion)
                    ? producto.Descripcion
                    : $"{producto.Producto} - {producto.Presentacion}",

                idProducto = producto.IdProducto,
                idPresentacion = producto.IdPresentacion,
                producto = producto.Producto,
                presentacion = producto.Presentacion,
                precio = producto.PrecioFinal
            }).ToList();

            return new JsonResult(listaNombres);
        }

        public async Task<IActionResult> OnGetObtenerDetalleProductoAsync(
            string frase,
            int idProducto,
            int idPresentacion)
        {
            if (idProducto <= 0 || idPresentacion <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Producto o presentación inválidos."
                });
            }

            PresentacionProductoVentaDTO? producto =
                await _productoAdapter.ObtenerPresentacionProductoByIdsAsync(idProducto, idPresentacion);

            if (producto == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró el producto."
                });
            }

            string nombreProducto = !string.IsNullOrWhiteSpace(producto.Producto)
                ? producto.Producto
                : !string.IsNullOrWhiteSpace(producto.Nombre)
                    ? producto.Nombre
                    : producto.Descripcion;

            string nombrePresentacion = !string.IsNullOrWhiteSpace(producto.Presentacion)
                ? producto.Presentacion
                : "Unidad";

            decimal precioUnitario = producto.PrecioFinal;

            return new JsonResult(new
            {
                success = true,
                producto = new
                {
                    idProducto = producto.IdProducto,
                    idPresentacion = producto.IdPresentacion,
                    nombre = nombreProducto,
                    nombreProducto = nombreProducto,
                    nombrePresentacion = nombrePresentacion,
                    precioUnitario = precioUnitario
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

        public async Task<JsonResult> OnGetEstadoVentaAsync(int idVenta)
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
                    message = "No se pudo obtener el estado de la venta."
                });
            }

            string estado = ventaCompleta.Venta.EstadoVenta;

            return new JsonResult(new
            {
                success = true,
                idVenta = ventaCompleta.Venta.Id,
                estado = estado,
                textoEstado = ObtenerTextoEstadoUsuario(estado),
                finalizada = EsEstadoFinal(estado),
                confirmada = estado == "CONFIRMADA",
                noCompletada = estado == "STOCK_RECHAZADO" || estado == "FALLIDA",
                anulada = estado == "ANULADA",
                pendiente = estado == "PENDIENTE" ||
                            estado == "STOCK_RESERVADO" ||
                            estado == "ANULACION_PENDIENTE"
            });
        }

        public class RegistrarVentaDto
        {
            public int IdCliente { get; set; }

            public string RazonSocial { get; set; } = string.Empty;

            public string Ci { get; set; } = string.Empty;

            public string? Complemento { get; set; }

            public string? Email { get; set; }

            public bool ClienteFrecuente { get; set; }

            public List<DetalleVentaDTO> Detalles { get; set; } = new();
        }

        public async Task<JsonResult> OnPostRegistrarVentaAsync([FromBody] RegistrarVentaDto dto)
        {
            if (dto == null)
                return new JsonResult(new { success = false, message = "Solicitud inválida." });

            if (dto.IdCliente <= 0)
                return new JsonResult(new { success = false, message = "Cliente no válido." });

            if (string.IsNullOrWhiteSpace(dto.RazonSocial))
                return new JsonResult(new { success = false, message = "Debe seleccionar un cliente válido. No llegó la razón social." });

            if (string.IsNullOrWhiteSpace(dto.Ci))
                return new JsonResult(new { success = false, message = "Debe seleccionar un cliente válido. No llegó el CI." });

            if (dto.Detalles == null || !dto.Detalles.Any())
                return new JsonResult(new { success = false, message = "La venta no tiene productos." });

            int idUsuario = ObtenerIdUsuario();

            if (idUsuario <= 0)
                return new JsonResult(new { success = false, message = "No se pudo identificar al usuario actual." });

            List<DetalleVentaDTO> detallesCorregidos = new();

            foreach (DetalleVentaDTO detalle in dto.Detalles)
            {
                if (detalle.IdProducto <= 0 || detalle.IdPresentacion <= 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Existe un producto inválido en la venta."
                    });
                }

                if (detalle.Cantidad <= 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "La cantidad de un producto debe ser mayor a cero."
                    });
                }

                PresentacionProductoVentaDTO? producto =
                    await _productoAdapter.ObtenerPresentacionProductoByIdsAsync(
                        detalle.IdProducto,
                        detalle.IdPresentacion
                    );

                string nombreProducto = !string.IsNullOrWhiteSpace(detalle.NombreProducto)
                    ? detalle.NombreProducto
                    : producto != null && !string.IsNullOrWhiteSpace(producto.Producto)
                        ? producto.Producto
                        : producto != null && !string.IsNullOrWhiteSpace(producto.Nombre)
                            ? producto.Nombre
                            : producto != null && !string.IsNullOrWhiteSpace(producto.Descripcion)
                                ? producto.Descripcion
                                : string.Empty;

                string nombrePresentacion = !string.IsNullOrWhiteSpace(detalle.NombrePresentacion)
                    ? detalle.NombrePresentacion
                    : producto != null && !string.IsNullOrWhiteSpace(producto.Presentacion)
                        ? producto.Presentacion
                        : string.Empty;

                decimal precioUnitario = detalle.PrecioUnitario;

                if (precioUnitario <= 0 && producto != null)
                    precioUnitario = producto.PrecioFinal;

                if (string.IsNullOrWhiteSpace(nombreProducto))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se pudo obtener el nombre del producto para registrar la venta."
                    });
                }

                if (string.IsNullOrWhiteSpace(nombrePresentacion))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se pudo obtener la presentación del producto para registrar la venta."
                    });
                }

                if (precioUnitario < 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"El precio del producto {nombreProducto} no puede ser negativo."
                    });
                }

                detallesCorregidos.Add(new DetalleVentaDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdPresentacion = detalle.IdPresentacion,
                    NombreProducto = nombreProducto,
                    NombrePresentacion = nombrePresentacion,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = detalle.Cantidad * precioUnitario
                });
            }

            var request = new RegistrarVentaRequestDTO
            {
                Venta = new VentaRegistroDTO
                {
                    IdCliente = dto.IdCliente,
                    IdUsuario = idUsuario
                },
                Cliente = new ClienteVentaSnapshotRequestDTO
                {
                    IdCliente = dto.IdCliente,
                    RazonSocial = dto.RazonSocial,
                    Ci = dto.Ci,
                    Complemento = string.IsNullOrWhiteSpace(dto.Complemento) ? null : dto.Complemento,
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                    ClienteFrecuente = dto.ClienteFrecuente
                },
                Detalles = detallesCorregidos
            };

            ApiResultDTO<ResultadoInicioVentaSagaDTO>? result =
                await _ventaAdapter.RegistrarVentaAsync(request);

            if (result != null && result.IsSuccess && result.Value != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    idVenta = result.Value.IdVenta,
                    estado = result.Value.Estado,
                    message = "Venta recibida correctamente."
                });
            }

            string mensajeError = result?.Error
                ?? result?.Errors.FirstOrDefault()
                ?? "Error al registrar la venta.";

            return new JsonResult(new
            {
                success = false,
                message = mensajeError
            });
        }

        private string ObtenerTextoEstadoUsuario(string estado)
        {
            return estado switch
            {
                "PENDIENTE" => "Procesando",
                "STOCK_RESERVADO" => "Procesando",
                "ANULACION_PENDIENTE" => "Procesando anulación",
                "CONFIRMADA" => "Confirmada",
                "ANULADA" => "Anulada",
                "STOCK_RECHAZADO" => "No completada",
                "FALLIDA" => "No completada",
                _ => "Procesando"
            };
        }

        private bool EsEstadoFinal(string estado)
        {
            return estado == "CONFIRMADA" ||
                   estado == "ANULADA" ||
                   estado == "STOCK_RECHAZADO" ||
                   estado == "FALLIDA";
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