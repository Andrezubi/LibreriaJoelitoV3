using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    /// 
    /// Valida un segmento del nombre de una persona (nombre, apellido paterno, apellido materno).
    /// Reglas:
    /// - No puede estar vacío.
    /// - Solo letras (incluyendo acentos y ñ), espacios y guiones (para nombres compuestos).
    /// - Mínimo 2 caracteres, máximo según el límite indicado.
    /// - No puede empezar ni terminar con espacio o guion.
    /// - No puede tener dos espacios o guiones consecutivos.
    /// 
    public sealed class NombrePersona
    {
        public string Valor { get; }

        private NombrePersona(string valor) => Valor = valor;

        public static Resultado<NombrePersona> Crear(string? valor, string nombreCampo = "Nombre", int maxLength = 100, bool esOpcional = false)
            {
                if (string.IsNullOrWhiteSpace(valor))
                {
                    if (esOpcional)
                        return Resultado.Exitoso(new NombrePersona(string.Empty));

                    return Resultado.Fallido<NombrePersona>(
                        Error.Validacion($"El campo '{nombreCampo}' no puede estar vacío."));
                }

                var limpio = valor.Trim();

            if (limpio.Length < 2)
                return Resultado.Fallido<NombrePersona>(
                    Error.Validacion($"'{nombreCampo}' debe tener al menos 2 caracteres."));

            if (limpio.Length > maxLength)
                return Resultado.Fallido<NombrePersona>(
                    Error.Validacion($"'{nombreCampo}' no puede superar {maxLength} caracteres."));

            // Solo letras con acento, ñ, espacios y guiones
            if (!Regex.IsMatch(limpio, @"^[\p{L}\s\-]+$"))
                return Resultado.Fallido<NombrePersona>(
                    Error.Validacion($"'{nombreCampo}' solo puede contener letras, espacios y guiones."));

            // No puede empezar o terminar con guion
            if (limpio.StartsWith('-') || limpio.EndsWith('-'))
                return Resultado.Fallido<NombrePersona>(
                    Error.Validacion($"'{nombreCampo}' no puede empezar ni terminar con guion."));

            // Sin espacios o guiones dobles consecutivos
            if (limpio.Contains("  ") || limpio.Contains("--"))
                return Resultado.Fallido<NombrePersona>(
                    Error.Validacion($"'{nombreCampo}' no puede tener espacios o guiones consecutivos."));

            // Capitalizar correctamente: primera letra de cada palabra en mayúscula
            var capitalizado = CapitalizarNombre(limpio);

            return Resultado.Exitoso(new NombrePersona(capitalizado));
        }

        /// 
        /// Capitaliza cada palabra del nombre respetando partículas como "de", "del", "la".
        /// Ej: "maria DEL carmen" → "Maria del Carmen"
        /// 
        private static string CapitalizarNombre(string nombre)
        {
            var particulas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "de", "del", "la", "las", "los", "y", "e" };

            var palabras = nombre.ToLower().Split(' ');
            var resultado = palabras.Select((p, i) =>
                i == 0 || !particulas.Contains(p)
                    ? char.ToUpper(p[0]) + p[1..]
                    : p
            );

            return string.Join(" ", resultado);
        }

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is NombrePersona n && n.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }

}
