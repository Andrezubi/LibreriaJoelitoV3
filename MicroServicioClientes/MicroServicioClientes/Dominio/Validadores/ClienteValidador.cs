using MicroServicioClientes.Dominio.Modelos;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroServicioClientes.Dominio.Validadores
{
    public class ClienteValidador
    {
        public string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }

        public List<ValidationResult> Validar(Cliente cliente)
        {
            var errores = new List<ValidationResult>();

            ValidarRazonSocial(cliente.RazonSocial, errores);
            ValidarCI(cliente.Ci, errores);
            ValidarComplemento(cliente.Complemento, errores);
            ValidarEmail(cliente.Email, errores);

            return errores;
        }

        void ValidarRazonSocial(string razonSocial, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
            {
                errores.Add(new ValidationResult("La Razón Social es obligatoria.", new[] { "_cliente.RazonSocial" }));
                return;
            }
            if (!Regex.IsMatch(razonSocial, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                errores.Add(new ValidationResult("La Razón Social solo puede contener letras y espacios.", new[] { "_cliente.RazonSocial" }));
            }
        }
        void ValidarCI(string ci, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(ci))
            {
                errores.Add(new ValidationResult("El CI es obligatorio."));
                return;
            }

            if (!Regex.IsMatch(ci, @"^\d+$"))
            {
                errores.Add(new ValidationResult("El CI solo puede contener números."));
                return; 
            }

            if (ci.Length < 6 || ci.Length > 10)
            {
                errores.Add(new ValidationResult("El CI debe tener entre 6 y 10 caracteres."));
                return;
            }

            if (Regex.IsMatch(ci, @"^0{3,}"))
            {
                errores.Add(new ValidationResult("El CI no es válido."));
                return;
            }

            if (long.TryParse(ci, out long valor) && valor < 10000)
            {
                errores.Add(new ValidationResult("El CI ingresado no parece válido."));
                return;
            }
        }

        void ValidarEmail(string? email, List<ValidationResult> errores)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errores.Add(new ValidationResult("El Email no tiene un formato válido.", new[] { "_cliente.Email" }));
                }
            }
        }

        void ValidarComplemento(string? complemento, List<ValidationResult> errores)
        {
            if (!string.IsNullOrWhiteSpace(complemento))
            {
                if (!Regex.IsMatch(complemento, @"^[0-9][A-Z]$"))
                {
                    errores.Add(new ValidationResult("El Complemento debe tener un formato de número seguido de letra mayúscula (ej: 1A).", new[] { "_cliente.Complemento" }));
                }
            }
        }
    }
}
