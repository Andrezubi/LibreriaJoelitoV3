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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _usuarioAdapter.Insertar(NuevoUsuario);
            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = "Usuario registrado exitosamente. Se ha enviado un correo con sus credenciales.";
                return RedirectToPage("/Usuarios/UsuarioIndex");
            }

            foreach (var error in resultado.Errores)
            {
                // La API puede devolver múltiples errores separados por '|' en un solo string
                var erroresIndividuales = error.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var errorIndividual in erroresIndividuales)
                {
                    var campo = MapearErrorACampo(errorIndividual);
                    ModelState.AddModelError(campo, errorIndividual);
                }
            }
            return Page();
        }
        private string MapearErrorACampo(string error)
        {
            var errorLower = error.ToLower();

            if (errorLower.Contains("apellido paterno"))
                return "NuevoUsuario.ApellidoPaterno";
            if (errorLower.Contains("apellido materno"))
                return "NuevoUsuario.ApellidoMaterno";
            if (errorLower.Contains("nombre"))
                return "NuevoUsuario.Nombre";
            if (errorLower.Contains("ci ") || errorLower.Contains("ci debe") || errorLower.Contains("el ci"))
                return "NuevoUsuario.Ci";
            if (errorLower.Contains("email") || errorLower.Contains("correo"))
                return "NuevoUsuario.Email";
            if (errorLower.Contains("teléfono") || errorLower.Contains("telefono"))
                return "NuevoUsuario.Telefono";
            if (errorLower.Contains("rol"))
                return "NuevoUsuario.Rol";
            if (errorLower.Contains("edad") || errorLower.Contains("nacimiento") || errorLower.Contains("18 años"))
                return "NuevoUsuario.FechaNacimiento";

            return string.Empty;
        }
    }
}
