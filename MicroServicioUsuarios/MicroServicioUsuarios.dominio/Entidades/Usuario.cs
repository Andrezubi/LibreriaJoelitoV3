using MicroServicioUsuarios.dominio.EntidadesDeValor;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.Entidades
{
    /// <summary>
    /// Entidad Usuario.
    /// Sus propiedades almacenan los VALUES extraídos de los Value Objects —
    /// EF Core mapea tipos primitivos, no Value Objects directamente.
    /// La construcción siempre pasa por la UsuarioFabrica, que valida todo
    /// mediante Value Objects antes de llegar aquí.
    /// </summary>
    public class Usuario
    {
        public int Id { get; private set; }
        public string NombreUsuario { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool MustChangePassword { get; private set; }
        public string Rol { get; private set; } = string.Empty;
        public bool Estado { get; private set; }

        // ── Datos personales (valores extraídos de Value Objects) ─────────
        public string Nombre { get; private set; } = string.Empty;
        public string ApellidoPaterno { get; private set; } = string.Empty;
        public string ApellidoMaterno { get; private set; } = string.Empty;
        public string Ci { get; private set; } = string.Empty;
        public string? Complemento { get; private set; }
        public DateOnly FechaNacimiento { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string DireccionDomicilio { get; private set; } = string.Empty;
        public string Telefono { get; private set; } = string.Empty;
        public DateOnly FechaIngreso { get; private set; }

        // ── Auditoría ────────────────────────────────────────────────────────
        public DateTime FechaRegistro { get; private set; }
        public DateTime? FechaUltimaActualizacion { get; private set; }
        public int IdUsuario { get; private set; }

        // Constructor privado para EF Core
        private Usuario() { }

        internal Usuario(
            string nombreUsuario,
            string passwordHash,
            string rol,
            NombrePersona nombre,
            NombrePersona apellidoPaterno,
            NombrePersona apellidoMaterno,
            CarnetIdentidad ci,
            FechaNacimiento fechaNacimiento,
            Email email,
            Direccion direccion,
            Telefono telefono,
            FechaIngreso fechaIngreso,
            int idUsuarioRegistrador)
        {
            NombreUsuario = nombreUsuario;
            PasswordHash = passwordHash;
            MustChangePassword = true;
            Rol = rol;
            Estado = true;

            Nombre = nombre.Valor;
            ApellidoPaterno = apellidoPaterno.Valor;
            ApellidoMaterno = apellidoMaterno.Valor;
            Ci = ci.Numero;
            Complemento = ci.Complemento;
            FechaNacimiento = fechaNacimiento.Valor;
            Email = email.Valor;
            DireccionDomicilio = direccion.Valor;
            Telefono = telefono.Valor;
            FechaIngreso = fechaIngreso.Valor;

            FechaRegistro = DateTime.UtcNow;
            IdUsuario = idUsuarioRegistrador;
        }

        // ── Métodos de negocio ───────────────────────────────────────────────

        public void ActualizarPassword(string nuevoHash, int idModificador)
        {
            PasswordHash = nuevoHash;
            MustChangePassword = false;
            FechaUltimaActualizacion = DateTime.UtcNow;
            IdUsuario = idModificador;
        }

        public void ForzarCambioPassword()
        {
            MustChangePassword = true;
            FechaUltimaActualizacion = DateTime.UtcNow;
        }

        public void Actualizar(
            NombrePersona nombre,
            NombrePersona apellidoPaterno,
            NombrePersona apellidoMaterno,
            Email email,
            Direccion direccion,
            Telefono telefono,
            string rol,
            int idModificador)
        {
            Nombre = nombre.Valor;
            ApellidoPaterno = apellidoPaterno.Valor;
            ApellidoMaterno = apellidoMaterno.Valor;
            Email = email.Valor;
            DireccionDomicilio = direccion.Valor;
            Telefono = telefono.Valor;
            Rol = rol;
            FechaUltimaActualizacion = DateTime.UtcNow;
            IdUsuario = idModificador;
        }

        public void Desactivar(int idModificador)
        {
            Estado = false;
            FechaUltimaActualizacion = DateTime.UtcNow;
            IdUsuario = idModificador;
        }

        public void Activar(int idModificador)
        {
            Estado = true;
            FechaUltimaActualizacion = DateTime.UtcNow;
            IdUsuario = idModificador;
        }

        public string NombreCompleto =>
            $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

        public string CiCompleto =>
            Complemento is null ? Ci : $"{Ci} {Complemento}";
    }
}
