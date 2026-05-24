using FrontendLibreria.Adaptadores;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendLibreria.Pages.Usuarios
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioIndexModel : PageModel
    {
        private readonly IUsuarioServicioAdapter _usuarioAdapter;

        public UsuarioIndexModel(IUsuarioServicioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        public List<UsuarioDto> Usuarios { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Usuarios = await _usuarioAdapter.ObtenerTodos();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var exito = await _usuarioAdapter.Eliminar(id);
            if (exito)
            {
                SuccessMessage = "Usuario eliminado (baja lógica) exitosamente.";
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el usuario.";
            }

            return RedirectToPage();
        }
    }
}
