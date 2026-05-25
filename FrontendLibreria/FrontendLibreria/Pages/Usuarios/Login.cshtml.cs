using FrontendLibreria.Adaptadores;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Usuarios
{
    public class LoginModel : PageModel
    {
        private readonly IUsuarioServicioAdapter _usuarioServicio;

        public LoginModel(IUsuarioServicioAdapter usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        [BindProperty]
        public LoginRequestDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _usuarioServicio.LoginAsync(Input);

            if (result == null)
            {
                ErrorMessage = "Credenciales inválidas o cuenta desactivada.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.NombreUsuario),
                new Claim(ClaimTypes.Role, result.Rol),
                new Claim("Token", result.Token),
                new Claim("NombreCompleto", result.NombreCompleto)
            };

            // Marcar si necesita cambiar password
            if (result.MustChangePassword)
            {
                claims.Add(new Claim("MustChangePassword", "true"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Si es primer inicio, redirigir obligatoriamente a CambiarPassword
            if (result.MustChangePassword)
            {
                return RedirectToPage("/Usuarios/CambiarPassword");
            }

            return RedirectToPage("/Index");
        }
    }
}
