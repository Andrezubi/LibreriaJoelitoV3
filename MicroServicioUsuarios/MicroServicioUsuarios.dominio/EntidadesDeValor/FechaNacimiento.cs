using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.EntidadesDeValor
{
    /// <summary>
    /// Fecha de nacimiento con reglas de negocio:
    /// - No puede ser futura.
    /// - El usuario debe tener al menos 18 años.
    /// - No puede ser anterior al año 1900.
    /// </summary>
    public sealed class FechaNacimiento
    {
        public DateOnly Valor { get; }
        public int Edad => CalcularEdad();

        private FechaNacimiento(DateOnly valor) => Valor = valor;

        public static Resultado<FechaNacimiento> Crear(DateOnly fecha)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            if (fecha > hoy)
                return Resultado.Fallido<FechaNacimiento>(
                    Error.Validacion("La fecha de nacimiento no puede ser futura."));

            if (fecha.Year < 1900)
                return Resultado.Fallido<FechaNacimiento>(
                    Error.Validacion("La fecha de nacimiento no puede ser anterior al año 1900."));

            var edad = CalcularEdad(fecha, hoy);
            if (edad < 18)
                return Resultado.Fallido<FechaNacimiento>(
                    Error.Validacion($"El usuario debe tener al menos 18 años. Edad calculada: {edad}."));

            return Resultado.Exitoso(new FechaNacimiento(fecha));
        }

        public static Resultado<FechaNacimiento> Crear(int anio, int mes, int dia)
        {
            try
            {
                return Crear(new DateOnly(anio, mes, dia));
            }
            catch (ArgumentOutOfRangeException)
            {   
                return Resultado.Fallido<FechaNacimiento>(
                    Error.Validacion($"La fecha {dia}/{mes}/{anio} no es válida."));
            }
        }

        private int CalcularEdad() =>
            CalcularEdad(Valor, DateOnly.FromDateTime(DateTime.UtcNow));

        private static int CalcularEdad(DateOnly nacimiento, DateOnly hoy)
        {
            var edad = hoy.Year - nacimiento.Year;
            if (nacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        public override string ToString() => Valor.ToString("yyyy-MM-dd");
        public override bool Equals(object? obj) => obj is FechaNacimiento f && f.Valor == Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }
}
