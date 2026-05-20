using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    public sealed class PoliticaContraseña
    {
        private PoliticaContraseña { }

        /// <summary>
        /// Valida que una contraseña cumpla la política de seguridad:
        /// mínimo 8 caracteres, al menos 1 número, 1 mayúscula, 1 minúscula, 1 carácter especial.
        /// </summary>
        public static Result Validar(string password)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                errores.Add("Debe tener al menos 8 caracteres.");

            if (!password.Any(char.IsUpper))
                errores.Add("Debe contener al menos una letra mayúscula.");

            if (!password.Any(char.IsLower))
                errores.Add("Debe contener al menos una letra minúscula.");

            if (!password.Any(char.IsDigit))
                errores.Add("Debe contener al menos un número.");

            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;':\",./<>?".Contains(c)))
                errores.Add("Debe contener al menos un carácter especial.");

            if (errores.Any())
                return Result.Fallido(Error.Validacion(string.Join(" ", errores)));

            return Result.Exitoso();
        }
    }

