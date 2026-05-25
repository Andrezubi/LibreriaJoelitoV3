using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.Resultados
{
    public sealed class Error
    {
        public string Codigo { get; }
        public string Mensaje { get; }
        public TipoError Tipo { get; }

        private Error(string codigo, string mensaje, TipoError tipo)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Tipo = tipo;
        }

        public static Error Validacion(string mensaje) =>
        new("VALIDACION", mensaje, TipoError.Validacion);

        public static Error NoEncontrado(string mensaje) =>
            new("NO_ENCONTRADO", mensaje, TipoError.NoEncontrado);

        public static Error Conflicto(string mensaje) =>
            new("CONFLICTO", mensaje, TipoError.Conflicto);

        public static Error NoAutorizado(string mensaje) =>
            new("NO_AUTORIZADO", mensaje, TipoError.NoAutorizado);

        public static Error Interno(string mensaje) =>
            new("INTERNO", mensaje, TipoError.Interno);

        public static readonly Error Ninguno = new(string.Empty, string.Empty, TipoError.Ninguno);

        public override string ToString() => $"[{Codigo}] {Mensaje}";
    }

    public enum TipoError
    {
        Ninguno,
        Validacion,
        NoEncontrado,
        Conflicto,
        NoAutorizado,
        Interno
    }

}

