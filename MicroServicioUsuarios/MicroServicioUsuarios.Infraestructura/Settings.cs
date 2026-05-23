using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura
{
    public sealed class JwtSettings
    {
        public const string Seccion = "JwtSettings";
        public string ClaveSecreta { get; init; } = string.Empty;
        public string Emisor { get; init; } = "MicroservicioUsuario";
        public string Audiencia { get; init; } = "FrontendRazor";
        public int ExpiracionHoras { get; init; } = 8;
    }

    public sealed class EmailSettings
    {
        public const string Seccion = "EmailSettings";
        public string Servidor { get; init; } = string.Empty;
        public int Puerto { get; init; } = 587;
        public string Usuario { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string RemitAlias { get; init; } = "Sistema de Usuarios";
    }
}
