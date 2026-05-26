using FrontendLibreria.Adaptadores.ProveedoresAdapter;
using FrontendLibreria.DTOs.Proveedores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;


namespace FrontendLibreria.Pages.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class IndexProveedorModel : PageModel
    {
        private readonly IProveedorAdapter _proveedorAdapter;

        public IndexProveedorModel(IProveedorAdapter proveedorAdapter)
        {
            _proveedorAdapter = proveedorAdapter;
        }

        public List<ProveedorDto> Proveedores { get; set; } = new();

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGet()
        {

            Proveedores = await _proveedorAdapter.ObtenerTodosAsync();
        }

        public async Task<IActionResult> OnGetObtenerProveedor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se recibió el ID del proveedor."
                });
            }

            var proveedor = await _proveedorAdapter.ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró el proveedor solicitado."
                });
            }

            return new JsonResult(new
            {
                success = true,
                proveedor
            });
        }

        public async Task<IActionResult> OnPostActualizarProveedorAsync([FromBody] RegistrarProveedorDto proveedor)
        {
            if (proveedor == null || string.IsNullOrWhiteSpace(proveedor.Id))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Los datos del proveedor no son válidos."
                });
            }

            proveedor.IdUsuario = ObtenerIdUsuario();

            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El nombre del proveedor es obligatorio."
                });
            }

            if (proveedor.Nit <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El NIT del proveedor es obligatorio."
                });
            }

            if (proveedor.TelefonoContacto <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El teléfono de contacto es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(proveedor.Direccion))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "La dirección del proveedor es obligatoria."
                });
            }

            var actualizado = await _proveedorAdapter.ActualizarAsync(proveedor.Id, proveedor);

            if (!actualizado)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se pudo actualizar el proveedor."
                });
            }

            TempData["MensajeExito"] = "Proveedor actualizado exitosamente.";

            return new JsonResult(new
            {
                success = true,
                message = "Proveedor actualizado exitosamente."
            });
        }

        public async Task<IActionResult> OnPostEliminarProveedorAsync([FromBody] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se recibió el ID del proveedor."
                });
            }

            var eliminado = await _proveedorAdapter.EliminarAsync(id);

            if (!eliminado)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No se pudo eliminar el proveedor."
                });
            }

            TempData["MensajeExito"] = "Proveedor eliminado exitosamente.";

            return new JsonResult(new
            {
                success = true,
                message = "Proveedor eliminado exitosamente."
            });
        }

        private int ObtenerIdUsuario()
        {
            return 1;
        }
    }
}