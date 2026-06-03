using System.ComponentModel.DataAnnotations;

namespace FrontendLibreria.DTOs
{
    public class SolicitudCrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApellidoPaterno { get; set; } = "";

        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [RegularExpression("^[1-9]\\d*$", ErrorMessage = "El CI no puede comenzar con ceros.")]
        public string Ci { get; set; } = "";

        public string? Complemento { get; set; }
        public string? DireccionDomicilio { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        public string Email { get; set; } = "";

        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Rol { get; set; } = "";

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public string? FechaNacimiento { get; set; }
    }
}
