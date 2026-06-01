using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    public sealed class Direccion
    {
        public string Valor { get; }

        private Direccion(string valor) => Valor = valor;

        public static Resultado<Direccion> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Resultado.Fallido<Direccion>(
                    Error.Validacion("La dirección no puede estar vacía."));

            var limpio = valor.Trim();

            if (limpio.Length < 5)
                return Resultado.Fallido<Direccion>(
                    Error.Validacion("La dirección debe tener al menos 5 caracteres."));

            if (limpio.Length > 200)
                return Resultado.Fallido<Direccion>(
                    Error.Validacion("La dirección no puede superar 200 caracteres."));

            return Resultado.Exitoso(new Direccion(limpio));
        }

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is Direccion d && d.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }


    public sealed class FechaIngreso
    {
        public DateOnly Valor { get; }

        private FechaIngreso(DateOnly valor) => Valor = valor;

        public static Resultado<FechaIngreso> Crear(DateOnly fecha)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            if (fecha > hoy)
                return Resultado.Fallido<FechaIngreso>(
                    Error.Validacion("La fecha de ingreso no puede ser futura."));

            ///Revisar mas adelante si se quiere permitir fechas de ingreso anteriores al año 2000, o si se quiere dejar esa validación para el caso de uso.
            if (fecha.Year < 2000)
                return Resultado.Fallido<FechaIngreso>(
                    Error.Validacion("La fecha de ingreso no puede ser anterior al año 2000."));

            return Resultado.Exitoso(new FechaIngreso(fecha));
        }

        /// Valida que la fecha de ingreso sea posterior a la de nacimiento.
        public Resultado ValidarCoherenciaConNacimiento(FechaNacimiento nacimiento)
        {
            if (Valor <= nacimiento.Valor)
                return Resultado.Fallido(
                    Error.Validacion("La fecha de ingreso debe ser posterior a la fecha de nacimiento."));

            return Resultado.Exitoso();
        }

        public override string ToString() => Valor.ToString("yyyy-MM-dd");
        public override bool Equals(object? obj) => obj is FechaIngreso f && f.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }

}
