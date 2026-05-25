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

            var error = await CambiarPasswordAsync();
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            // Al cerrar la sesión, el siguiente acceso exige utilizar la nueva contraseña.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            SuccessMessage = "¡Contraseña actualizada exitosamente! Serás redirigido al login...";
            ViewData["Redirect"] = true;

            return Page();
        }

        public async Task<IActionResult> OnPostVoluntarioAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { exito = false, mensaje = "Complete todos los campos requeridos." });
            }

            var error = await CambiarPasswordAsync();
            if (error is not null)
            {
                return BadRequest(new { exito = false, mensaje = error });
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return new JsonResult(new
            {
                exito = true,
                mensaje = "Contraseña actualizada exitosamente. Inicie sesión nuevamente."
            });
        }

        private async Task<string?> CambiarPasswordAsync()
        {
            if (Input.NuevoPassword != Input.ConfirmarPassword)
            {
                return "La nueva contraseña y su confirmación no coinciden.";
            }

            var (exito, errores) = await _usuarioServicio.CambiarPasswordAsync(Input);
            if (exito)
            {
                return null;
            }

            return errores.FirstOrDefault()
                ?? "Error al cambiar la contraseña. Verifique que cumpla con las políticas de seguridad.";
        }
    }
}
