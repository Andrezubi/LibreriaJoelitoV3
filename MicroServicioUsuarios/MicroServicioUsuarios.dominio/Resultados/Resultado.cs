using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.Resultados
{
    public class Resultado
    {
        protected Resultado(bool esExitoso, Error error)
        {
            if (esExitoso && error != Error.Ninguno)
                throw new InvalidOperationException("Un resultado exitoso no puede tener error.");
            if (!esExitoso && error == Error.Ninguno)
                throw new InvalidOperationException("Un resultado fallido debe tener un error.");

            EsExitoso = esExitoso;
            Error = error;
        }

        public bool EsExitoso { get; }
        public bool EsFallido => !EsExitoso;
        public Error Error { get; }

        public static Resultado Exitoso() => new(true, Error.Ninguno);
        public static Resultado Fallido(Error error) => new(false, error);

        public static Resultado<TValor> Exitoso<TValor>(TValor valor) =>
            new(valor, true, Error.Ninguno);

        public static Resultado<TValor> Fallido<TValor>(Error error) =>
            new(default!, false, error);
    }

    public sealed class Resultado<TValor> : Resultado
    {
        private readonly TValor _valor;

        internal Resultado(TValor valor, bool esExitoso, Error error)
            : base(esExitoso, error)
        {
            _valor = valor;
        }

        public TValor Valor => EsExitoso
            ? _valor
            : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");
    }
}
