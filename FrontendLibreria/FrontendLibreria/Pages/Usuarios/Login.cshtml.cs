using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FrontendLibreria.Adaptadores;

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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _usuarioServicio.LoginAsync(Input);

            if (result == null)
            {
                ErrorMessage = "Credenciales inválidas o cuenta desactivada.";
                return Page();
            }

            var idUsuario = ObtenerIdUsuarioDesdeToken(result.Token);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
        new Claim("IdUsuario", idUsuario.ToString()),
        new Claim(ClaimTypes.Name, result.NombreUsuario),
        new Claim(ClaimTypes.Role, result.Rol),
        new Claim("Token", result.Token),
        new Claim("NombreCompleto", result.NombreCompleto)
    };

            if (result.MustChangePassword)
            {
                claims.Add(new Claim("MustChangePassword", "true"));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            if (result.MustChangePassword)
            {
                return RedirectToPage("/Usuarios/CambiarPassword");
            }

            return RedirectToPage("/Index");
        }

        private static int ObtenerIdUsuarioDesdeToken(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var idClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "nameid" ||
                c.Type.EndsWith("/nameidentifier") ||
                c.Type == JwtRegisteredClaimNames.Sub
            );

            if (idClaim == null || !int.TryParse(idClaim.Value, out var idUsuario))
            {
                return 1;
            }

            return idUsuario;
        }
    }
}
