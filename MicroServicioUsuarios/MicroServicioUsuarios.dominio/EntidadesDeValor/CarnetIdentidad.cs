using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    /// <summary>
    /// Carnet de Identidad boliviano.
    /// Número: 5 a 8 dígitos.
    /// Complemento (opcional): 1 dígito + 1 letra mayúscula. Ej: "3D", "1A".
    /// </summary>
    public sealed class CarnetIdentidad
    {
        public string Numero { get; }
        public string? Complemento { get; }

        public string ValorCompleto => Complemento is null
            ? Numero
            : $"{Numero} {Complemento}";

        private CarnetIdentidad(string numero, string? complemento)
        {
            Numero = numero;
            Complemento = complemento;
        }

        public static Resultado<CarnetIdentidad> Crear(string numero, string? complemento = null)
        {
            // ── Validar número ──────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(numero))
                return Resultado.Fallido<CarnetIdentidad>(
                    Error.Validacion("El número de CI no puede estar vacío."));

            var numeroLimpio = numero.Trim();

            if (!Regex.IsMatch(numeroLimpio, @"^\d{8,11}$"))
                return Resultado.Fallido<CarnetIdentidad>(
                    Error.Validacion("El CI debe contener entre 8 y 11 dígitos numéricos."));

            // ── Validar complemento (si se proporcionó) ─────────────────────
            string? complementoLimpio = null;
            if (!string.IsNullOrWhiteSpace(complemento))
            {
                complementoLimpio = complemento.Trim().ToUpper();

                if (!Regex.IsMatch(complementoLimpio, @"^\d[A-Z]$"))
                    return Resultado.Fallido<CarnetIdentidad>(
                        Error.Validacion("El complemento del CI debe tener el formato: 1 dígito + 1 letra (ej: 3D, 1A)."));
            }

            return Resultado.Exitoso(new CarnetIdentidad(numeroLimpio, complementoLimpio));
        }

        /// <summary>Parsea un CI completo como string: "8051738" o "8051738 3D".</summary>
        public static Resultado<CarnetIdentidad> Parsear(string valorCompleto)
        {
            if (string.IsNullOrWhiteSpace(valorCompleto))
                return Resultado.Fallido<CarnetIdentidad>(Error.Validacion("El CI no puede estar vacío."));

            var partes = valorCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return partes.Length switch
            {
                1 => Crear(partes[0]),
                2 => Crear(partes[0], partes[1]),
                _ => Resultado.Fallido<CarnetIdentidad>(
                         Error.Validacion("Formato de CI inválido. Use: '8051738' o '8051738 3D'."))
            };
        }

        public override string ToString() => ValorCompleto;
        public override bool Equals(object? obj) => obj is CarnetIdentidad c && c.ValorCompleto == ValorCompleto;
        public override int GetHashCode() => ValorCompleto.GetHashCode();
    }
}
