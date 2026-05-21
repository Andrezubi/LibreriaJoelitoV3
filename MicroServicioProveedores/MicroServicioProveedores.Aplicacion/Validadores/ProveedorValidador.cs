using MicroServicioProveedores.Aplicacion.DTOs;
using MicroServicioProveedores.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Aplicacion.Validadores
{
    public class ProveedorValidador
    {
        public List<ValidationResult> Validar(RegistrarProveedorDto dto)
        {
            NormalizarDatos(dto);

            var errores = new List<ValidationResult>();

            ValidarTextosObligatorios(dto, errores);
            ValidarNit(dto.Nit, errores);
            ValidarTelefonoBoliviano(dto.TelefonoContacto, errores);
            ValidarAuditoria(dto.IdUsuario, errores);

            return errores;
        }

        private void NormalizarDatos(RegistrarProveedorDto dto)
        {
            dto.Nombre = NormalizarTexto(dto.Nombre);
            dto.Direccion = NormalizarTexto(dto.Direccion);
            dto.Descripcion = NormalizarDescripcion(dto.Descripcion);
        }

        private void ValidarTextosObligatorios(RegistrarProveedorDto dto, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                errores.Add(new ValidationResult("El nombre del proveedor es obligatorio."));
            }

            if (string.IsNullOrWhiteSpace(dto.Direccion))
            {
                errores.Add(new ValidationResult("La dirección es obligatoria."));
            }
        }

        private void ValidarNit(int nit, List<ValidationResult> errores)
        {
            if (nit <= 0)
            {
                errores.Add(new ValidationResult("El NIT proporcionado no es válido."));
                return; 
            }

            string nitStr = nit.ToString();
            if (nitStr.Length < 7 || nitStr.Length > 12)
            {
                errores.Add(new ValidationResult("El NIT debe tener una longitud válida según el formato de Impuestos Nacionales."));
            }
        }

        private void ValidarTelefonoBoliviano(int telefono, List<ValidationResult> errores)
        {
            string telefonoStr = telefono.ToString();

            if (telefonoStr.Length != 8)
            {
                errores.Add(new ValidationResult("El número de teléfono debe tener exactamente 8 dígitos."));
                return; 
            }

            if (!Regex.IsMatch(telefonoStr, "^[23467]"))
            {
                errores.Add(new ValidationResult("El teléfono debe empezar con 2, 3 o 4 (fijos) o con 6 o 7 (celulares)."));
            }
        }

        private void ValidarAuditoria(int idUsuario, List<ValidationResult> errores)
        {
            if (idUsuario <= 0)
            {
                errores.Add(new ValidationResult("El identificador del usuario responsable es inválido."));
            }
        }

        private string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }

        private string NormalizarDescripcion(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            return Regex.Replace(texto.Trim(), @"\s+", " ");
        }
    }
}
