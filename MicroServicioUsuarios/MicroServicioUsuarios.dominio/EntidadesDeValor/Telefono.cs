using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    /// <summary>
    /// Número de teléfono.
    /// Bolivia: celular 8 dígitos empezando en 6 o 7 (ej: 77012345).
    ///          fijo    7 dígitos empezando en 2, 3 o 4 (ej: 4123456).
    /// También acepta formato internacional con prefijo + (ej: +59177012345).
    /// </summary>
    public sealed class Telefono
    {
        public string Valor { get; }

        private Telefono(string valor) => Valor = valor;

        public static Resultado<Telefono> Crear(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return Resultado.Fallido<Telefono>(
                    Error.Validacion("El teléfono no puede estar vacío."));

            // Limpiar espacios, guiones y paréntesis
            var limpio = Regex.Replace(numero.Trim(), @"[\s\-()]", "");

            // Internacional: + seguido de 7 a 15 dígitos
            if (limpio.StartsWith("+"))
            {
                if (Regex.IsMatch(limpio, @"^\+\d{7,15}$"))
                    return Resultado.Exitoso(new Telefono(limpio));

                return Resultado.Fallido<Telefono>(
                    Error.Validacion("Formato internacional inválido. Ejemplo: +59177012345."));
            }

            // Celular boliviano: 8 dígitos, empieza en 6 o 7
            if (Regex.IsMatch(limpio, @"^[67]\d{7}$"))
                return Resultado.Exitoso(new Telefono(limpio));

            //Consultar la necesidad de implementar un formato para numeros fijos
            // Fijo boliviano: 7 dígitos, empieza en 2, 3 o 4
            if (Regex.IsMatch(limpio, @"^[234]\d{6}$"))
                return Resultado.Exitoso(new Telefono(limpio));

            return Resultado.Fallido<Telefono>(
                Error.Validacion("Teléfono inválido. Celular boliviano: 8 dígitos (6x o 7x). " +
                                 "Fijo: 7 dígitos. Internacional: +código país + número."));
        }

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is Telefono t && t.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }
}
