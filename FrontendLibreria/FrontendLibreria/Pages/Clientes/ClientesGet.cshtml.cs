using FrontendLibreria.Adaptadores;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Clientes
{
    //[Authorize(Roles = "Administrador,Empleado")]
    public class ClientesGetModel : PageModel
    {
        private readonly IAdaptadorCliente _adaptadorCliente;

        public ClientesGetModel(IAdaptadorCliente adaptadorCliente)
        {
            _adaptadorCliente = adaptadorCliente;
        }

        public List<ClienteDto> Clientes { get; set; } = new();

        [BindProperty]
        public ClienteDto ClienteEditar { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = await _adaptadorCliente.ObtenerTodoAsync();
        }

        public async Task<JsonResult> OnPostUpdateAsync()
        {
            try
            {
                ClienteEditar.Id = Convert.ToInt32(Request.Form["ClienteEditar.Id"]);

                
                ClienteEditar.IdUsuario = int.Parse(User.FindFirst("IdUsuario")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

                ClienteEditar.RazonSocial = Normalizar(ClienteEditar.RazonSocial)!;

                var resultado = await _adaptadorCliente.ActualizarAsync(ClienteEditar);

                if (!resultado.Success)
                {
                    string errorAgrupado = string.Join("<br/>• ", resultado.Errors);
                    return new JsonResult(new { success = false, message = "• " + errorAgrupado });
                }

                TempData["MensajeExito"] =
                    $"Cliente '{ClienteEditar.RazonSocial}' actualizado exitosamente.";

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
            await _adaptadorCliente.EliminarAsync(id, idUsuario);
            TempData["MensajeExito"] = "Cliente eliminado correctamente.";
            return RedirectToPage();
        }

        private static string? Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(texto.Trim().ToLower());
        }
    }
}