using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    /// <summary>
    /// Dirección de domicilio: texto libre con longitud controlada.
    /// </summary>
    public sealed class Direccion
    {
        public string Valor { get; }

        private Direccion(string valor) => Valor = valor;

        public static Result<Direccion> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result.Fallido<Direccion>(
                    Error.Validacion("La dirección no puede estar vacía."));

            var limpio = valor.Trim();

            if (limpio.Length < 5)
                return Result.Fallido<Direccion>(
                    Error.Validacion("La dirección debe tener al menos 5 caracteres."));

            if (limpio.Length > 200)
                return Result.Fallido<Direccion>(
                    Error.Validacion("La dirección no puede superar 200 caracteres."));

            return Result.Exitoso(new Direccion(limpio));
        }

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is Direccion d && d.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }

    /// <summary>
    /// Fecha de ingreso al sistema:
    /// - No puede ser futura.
    /// - No puede ser anterior al año 2000.
    /// - Debe ser >= fecha de nacimiento del usuario (se valida en el caso de uso).
    /// </summary>
    public sealed class FechaIngreso
    {
        public DateOnly Valor { get; }

        private FechaIngreso(DateOnly valor) => Valor = valor;

        public static Result<FechaIngreso> Crear(DateOnly fecha)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            if (fecha > hoy)
                return Result.Fallido<FechaIngreso>(
                    Error.Validacion("La fecha de ingreso no puede ser futura."));

            if (fecha.Year < 2000)
                return Result.Fallido<FechaIngreso>(
                    Error.Validacion("La fecha de ingreso no puede ser anterior al año 2000."));

            return Result.Exitoso(new FechaIngreso(fecha));
        }

        /// <summary>Valida que la fecha de ingreso sea posterior a la de nacimiento.</summary>
        public Result ValidarCoherenciaConNacimiento(FechaNacimiento nacimiento)
        {
            if (Valor <= nacimiento.Valor)
                return Result.Fallido(
                    Error.Validacion("La fecha de ingreso debe ser posterior a la fecha de nacimiento."));

            return Result.Exitoso();
        }

        public override string ToString() => Valor.ToString("yyyy-MM-dd");
        public override bool Equals(object? obj) => obj is FechaIngreso f && f.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }

}
