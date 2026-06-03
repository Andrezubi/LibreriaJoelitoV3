using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    public sealed class NombreUsuario
    {
        public string Valor { get; }

        private NombreUsuario(string valor) => Valor = valor;

        /// 
        /// Genera el nombre de usuario aplicando el protocolo:
        /// inicial del nombre + apellido + sufijo numérico si hay conflicto.
        /// Ej: Juan Pérez → jperez, jperez1, jperez2...
        /// 
        public static Resultado<NombreUsuario> Generar(string nombre, string apellido, int sufijo = 0)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Resultado.Fallido<NombreUsuario>(Error.Validacion("El nombre no puede estar vacío."));

            if (string.IsNullOrWhiteSpace(apellido))
                return Resultado.Fallido<NombreUsuario>(Error.Validacion("El apellido no puede estar vacío."));

            var inicial = Normalizar(nombre[0].ToString());
            var apellidoN = Normalizar(apellido);
            var base_ = $"{inicial}{apellidoN}";
            var valor = sufijo == 0 ? base_ : $"{base_}{sufijo}";

            return Resultado.Exitoso(new NombreUsuario(valor));
        }

        /// Valida un nombre de usuario ya existente.
        public static Resultado<NombreUsuario> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor) || valor.Length < 3)
                return Resultado.Fallido<NombreUsuario>(Error.Validacion("El nombre de usuario debe tener al menos 3 caracteres."));

            return Resultado.Exitoso(new NombreUsuario(valor.ToLower().Trim()));
        }

        private static string Normalizar(string texto) =>
            new string(texto
                .ToLower()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => c < 128 && char.IsLetterOrDigit(c))
                .ToArray());

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is NombreUsuario n && n.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }
}
