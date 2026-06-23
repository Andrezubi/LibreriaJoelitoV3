using FrontendLibreria.Adaptadores.Marca;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Marcas
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class VerMarcasModel : PageModel
    {
        private readonly IAdaptadorMarca _adaptadorMarca;

        public VerMarcasModel(IAdaptadorMarca adaptadorMarca)
        {
            _adaptadorMarca = adaptadorMarca;
        }

        public List<MarcaDto> Marcas { get; set; } = new();

        [BindProperty]
        public MarcaDto MarcaEditar { get; set; } = new();

        public async Task OnGetAsync()
        {
            Marcas = await _adaptadorMarca.ObtenerTodoAsync();
        }

        public async Task<JsonResult> OnPostUpdateAsync()
        {
            try
            {
                MarcaEditar.Id = Convert.ToInt32(Request.Form["MarcaEditar.Id"]);
                MarcaEditar.IdUsuario = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

                MarcaEditar.Nombre = Normalizar(MarcaEditar.Nombre)!;
                MarcaEditar.Industria = Normalizar(MarcaEditar.Industria);

                var resultado = await _adaptadorMarca.ActualizarAsync(MarcaEditar);

                if (!resultado.Success)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        errores = resultado.Errors
                    });
                }

                TempData["MensajeExito"] = $"Marca '{MarcaEditar.Nombre}' actualizada correctamente.";
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
            await _adaptadorMarca.EliminarAsync(id, idUsuario);
            TempData["MensajeExito"] = "Marca eliminada correctamente.";
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