using FrontendLibreria.Adaptadores.Marca;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Marcas
{
    //[Authorize(Roles = "Administrador,Empleado")]
    public class CrearMarcaModel : PageModel
    {
        private readonly IAdaptadorMarca _marcaAdapter;

        public CrearMarcaModel(IAdaptadorMarca marcaAdapter)
        {
            _marcaAdapter = marcaAdapter;
        }

        [BindProperty] public string Nombre { get; set; } = "";
        [BindProperty] public string? Industria { get; set; }
        [BindProperty] public string? PaginaWeb { get; set; }
        [BindProperty] public string? Descripcion { get; set; }

        [TempData] public string? MensajeExito { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            int idUsuario = ObtenerIdUsuario();

            var result = await _marcaAdapter.InsertarAsync(new MarcaDto
            {
                Nombre = Normalizar(Nombre)!,
                Industria = Normalizar(Industria),
                PaginaWeb = PaginaWeb,
                Descripcion = Descripcion,
                IdUsuario = idUsuario
            });

            if (!result.Success)
            {
                //foreach (var error in result.Errors)
                //    ModelState.AddModelError(string.Empty, error);

                foreach (var error in result.Errors)
                {
                    var campo = error.Campo ?? "";

                    campo = campo.Replace("marca.", "");
                    campo = campo.Replace("Marca.", "");

                    if (string.IsNullOrWhiteSpace(campo))
                    {
                        ModelState.AddModelError(string.Empty, error.Mensaje);
                    }
                    else
                    {
                        ModelState.AddModelError(campo, error.Mensaje);
                    }
                }

                return Page();
            }

            MensajeExito = $"Marca '{Nombre}' registrada exitosamente.";
            return RedirectToPage("VerMarcas");
        }

        /// <summary>
        /// Obtiene el ID del usuario autenticado desde los claims de la sesión.
        /// </summary>
        private int ObtenerIdUsuario()
        {
            var idClaim = User.FindFirst("IdUsuario")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? "0";
            
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        private static string? Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(texto.Trim().ToLower());
        }
    }
}