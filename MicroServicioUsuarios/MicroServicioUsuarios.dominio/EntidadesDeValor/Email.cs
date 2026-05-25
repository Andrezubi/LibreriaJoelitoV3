using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    public sealed class Email
    {
        public string Valor { get; }

        private Email(string valor) => Valor = valor;

        public static Resultado<Email> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Resultado.Fallido<Email>(Error.Validacion("El email no puede estar vacío."));

            // El estándar RFC 5321 establece que la longitud máxima de un email es de 254 caracteres.
            // Revisar en caso de uso específico si se desea una validación más estricta (ej. longitud del dominio, etc.)
            if (valor.Length > 254)
                return Resultado.Fallido<Email>(Error.Validacion("El email no puede superar 254 caracteres."));

            if (!Regex.IsMatch(valor, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Resultado.Fallido<Email>(Error.Validacion("El formato del email no es válido."));

            return Resultado.Exitoso(new Email(valor.Trim().ToLower()));
        }

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is Email e && e.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }

}
