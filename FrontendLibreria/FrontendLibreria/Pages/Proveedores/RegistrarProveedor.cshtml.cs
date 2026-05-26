using FrontendLibreria.Adaptadores.ProveedoresAdapter;
using FrontendLibreria.DTOs.Proveedores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class RegistrarProveedorModel : PageModel
    {
        private readonly IProveedorAdapter _proveedorAdapter;

        public RegistrarProveedorModel(IProveedorAdapter proveedorAdapter)
        {
            _proveedorAdapter = proveedorAdapter;
        }

        [BindProperty]
        public RegistrarProveedorDto Proveedor { get; set; } = new();

        [TempData]
        public string? MensajeExito { get; set; }


        public async Task<IActionResult> OnPostAsync()
        {
            PrepararDatosInternos();

            ValidarFormulario();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _proveedorAdapter.RegistrarConResultadoAsync(Proveedor);

            if (!resultado.Exitoso)
            {
                CargarErroresDelResultado(resultado);
                return Page();
            }

            TempData["MensajeExito"] = "Proveedor registrado exitosamente.";

            return RedirectToPage("IndexProveedor", new
            {
                refresh = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        private void PrepararDatosInternos()
        {
            var idClaim = User.FindFirst("IdUsuario")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            Proveedor.IdUsuario = int.Parse(idClaim);
        }

        private void ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Proveedor.Nombre))
            {
                ModelState.AddModelError(
                    "Proveedor.Nombre",
                    "El nombre del proveedor es obligatorio."
                );
            }

            if (Proveedor.Nit <= 0)
            {
                ModelState.AddModelError(
                    "Proveedor.Nit",
                    "El NIT del proveedor es obligatorio."
                );
            }

            if (Proveedor.TelefonoContacto <= 0)
            {
                ModelState.AddModelError(
                    "Proveedor.TelefonoContacto",
                    "El teléfono de contacto es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(Proveedor.Direccion))
            {
                ModelState.AddModelError(
                    "Proveedor.Direccion",
                    "La dirección del proveedor es obligatoria."
                );
            }

            if (Proveedor.IdUsuario <= 0)
            {
                ModelState.AddModelError(
                    "Proveedor.IdUsuario",
                    "El usuario responsable no es válido."
                );
            }
        }

        private void CargarErroresDelResultado(ProveedorOperacionResultadoDTO resultado)
        {
            foreach (var errorGeneral in resultado.ErroresGenerales)
            {
                ModelState.AddModelError(string.Empty, errorGeneral);
            }

            foreach (var errorCampo in resultado.ErroresPorCampo)
            {
                var key = $"Proveedor.{errorCampo.Key}";

                foreach (var mensaje in errorCampo.Value)
                {
                    ModelState.AddModelError(key, mensaje);
                }
            }
        }
    }
}