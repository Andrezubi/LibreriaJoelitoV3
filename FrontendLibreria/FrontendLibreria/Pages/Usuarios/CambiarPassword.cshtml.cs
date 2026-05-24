using FrontendLibreria.Adaptadores;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendLibreria.Pages.Usuarios
{
    [Authorize]
    public class CambiarPasswordModel : PageModel
    {
        private readonly IUsuarioServicioAdapter _usuarioServicio;

        public CambiarPasswordModel(IUsuarioServicioAdapter usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        [BindProperty]
        public CambiarPasswordDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Verificamos si es un cambio forzado (MustChangePassword)
            var mustChange = User.FindFirst("MustChangePassword")?.Value == "true";
            if (mustChange)
            {
                ViewData["Forzado"] = true;
                ViewData["MensajeForzado"] = "Por seguridad, debes cambiar tu contraseña antes de continuar.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (Input.NuevoPassword != Input.ConfirmarPassword)
            {
                ErrorMessage = "La nueva contraseña y su confirmación no coinciden.";
                return Page();
            }

            var (exito, errores) = await _usuarioServicio.CambiarPasswordAsync(Input);

            if (!exito)
            {
                if (errores != null && errores.Contains("⚠️ Contraseña actual incorrecta"))
                {
                    ErrorMessage = "La contraseña actual es incorrecta.";
                }
                else
                {
                    ErrorMessage = "Error al cambiar la contraseña. Verifique que la contraseña actual sea correcta y que la nueva cumpla con las políticas de seguridad.";
                }
                return Page();
            }

            // Si fue exitoso, cerramos sesión para que el usuario ingrese con su nueva clave (y se limpie el MustChangePassword claim)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            SuccessMessage = "¡Contraseña actualizada exitosamente! Serás redirigido al login...";
            ViewData["Redirect"] = true;

            return Page();
        }
    }
}
