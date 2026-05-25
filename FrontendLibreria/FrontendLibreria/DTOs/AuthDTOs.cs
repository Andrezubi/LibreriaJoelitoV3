namespace FrontendLibreria.DTOs
{
    public class LoginRequestDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
    }

    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string NuevoPassword { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
