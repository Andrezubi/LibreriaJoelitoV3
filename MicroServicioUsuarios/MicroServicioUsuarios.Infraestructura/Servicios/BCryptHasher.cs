using MicroServicioUsuarios.dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.Servicios
{
    /// <summary>
    /// Implementación de IPasswordHasher usando BCrypt.
    /// Registrado como Scoped — una instancia por request.
    /// </summary>
    public sealed class BcryptHasher : IContraHasher
    {
        private const int WorkFactor = 12;

        public string Hashear(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verificar(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);
    }

}
