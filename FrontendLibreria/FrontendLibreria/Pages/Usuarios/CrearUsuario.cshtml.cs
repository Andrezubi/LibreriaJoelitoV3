using FrontendLibreria.Adaptadores;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendLibreria.Pages.Usuarios
{
    [Authorize(Roles = "Administrador")]
    public class CrearUsuarioModel : PageModel
    {
        private readonly IUsuarioServicioAdapter _usuarioAdapter;

        public CrearUsuarioModel(IUsuarioServicioAdapter usuarioAdapter)
        {
            _usuarioAdapter = usuarioAdapter;
        }

        [BindProperty]
        public SolicitudCrearUsuarioDto NuevoUsuario { get; set; } = new();

        public List<string> ErrorMessages { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Page();
            }

            var resultado = await _usuarioAdapter.Insertar(NuevoUsuario);
            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = "Usuario registrado exitosamente. Se ha enviado un correo con sus credenciales.";
                return RedirectToPage("/Usuarios/UsuarioIndex");
            }

            ErrorMessages = resultado.Errores;
            return Page();
        }
    }
}
