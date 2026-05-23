using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.Servicios
{
    /// <summary>
    /// Singleton Lazy: la clave de firma y los parámetros de validación
    /// se construyen una única vez cuando se necesitan por primera vez.
    /// Patrón: Lazy[T] wrapping AddSingleton en DependencyInjection.cs
    /// </summary>
    public sealed class JwtServicio : IJwtServicio
    {
        private readonly Lazy<(SymmetricSecurityKey clave, TokenValidationParameters parametros)> _config;

        public JwtServicio(IOptions<JwtSettings> opciones)
        {
            // Lazy: construye la clave y parámetros solo al primer uso
            _config = new Lazy<(SymmetricSecurityKey, TokenValidationParameters)>(() =>
            {
                var clave = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(opciones.Value.ClaveSecreta));

                var parametros = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opciones.Value.Emisor,
                    ValidAudience = opciones.Value.Audiencia,
                    IssuerSigningKey = clave,
                    ClockSkew = TimeSpan.Zero
                };

                return (clave, parametros);
            });
        }

        public string Generar(int idUsuario, string nombreUsuario, string rol)
        {
            var (clave, _) = _config.Value;

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
            new Claim(ClaimTypes.Name, nombreUsuario),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "MicroservicioUsuario",
                audience: "FrontendRazor",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool Validar(string token)
        {
            var (_, parametros) = _config.Value;
            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
