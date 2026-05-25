using MicroServicioProductos.Dominio.Modelos;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroServicioProductos.Dominio.Validadores
{
    public class MarcaValidador
    {
        // Método para limpiar espacios extra y poner Formato De Título
        public string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }

        public List<ValidationResult> Validar(Marca marca)
        {
            var errores = new List<ValidationResult>();

            ValidarNombre(marca.Nombre, errores);
            ValidarDescripcion(marca.Descripcion, errores);
            ValidarPaginaWeb(marca.PaginaWeb, errores);
            ValidarIndustria(marca.Industria, errores);

            return errores;
        }

        private void ValidarNombre(string nombre, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                errores.Add(new ValidationResult("El Nombre de la marca es obligatorio.", new[] { "Marca.Nombre" }));
                return; 
            }

            if (nombre.Length < 2 || nombre.Length > 250)
            {
                errores.Add(new ValidationResult("El Nombre debe tener entre 2 y 250 caracteres.", new[] { "Marca.Nombre" }));
            }

            if (!Regex.IsMatch(nombre, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s&\.\-]+$"))
            {
                errores.Add(new ValidationResult("El Nombre contiene caracteres no permitidos.", new[] { "Marca.Nombre" }));
            }
        }

        private void ValidarDescripcion(string? descripcion, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                errores.Add(new ValidationResult("La Descripción de la marca es obligatoria.", new[] { "Marca.Descripcion" }));
                return;
            }

            if (descripcion.Length > 500)
            {
                errores.Add(new ValidationResult("La Descripción no puede exceder los 500 caracteres.", new[] { "Marca.Descripcion" }));
            }
        }

        private void ValidarPaginaWeb(string? url, List<ValidationResult> errores)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!Regex.IsMatch(url, @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$"))
                {
                    errores.Add(new ValidationResult("El formato de la Página Web no es válido.", new[] { "Marca.PaginaWeb" }));
                }
                if (url.Length > 250)
                {
                    errores.Add(new ValidationResult("La URL es demasiado larga.", new[] { "Marca.PaginaWeb" }));
                }
            }
        }

        private void ValidarIndustria(string? industria, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(industria))
            {
                errores.Add(new ValidationResult("La Industria es obligatoria.", new[] { "Marca.Industria" }));
                return;
            }

            if (industria.Length < 3 || industria.Length > 100)
            {
                errores.Add(new ValidationResult("El campo Industria debe tener entre 3 y 100 caracteres.", new[] { "Marca.Industria" }));
            }
        }
    }
}