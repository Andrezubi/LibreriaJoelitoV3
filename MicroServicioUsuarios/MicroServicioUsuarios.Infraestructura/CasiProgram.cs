using MicroServicioUsuarios.Aplicacion.CasosDeUso;
using MicroServicioUsuarios.Aplicacion.Fabrica;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.Infraestructura.Persistencia;
using MicroServicioUsuarios.Infraestructura.Servicios;
using MicroServicioUsuarios.dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MicroServicioUsuarios.Infraestructura.ConexionBD;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MicroServicioUsuarios.Infraestructura
{
    public static class CasiProgram
    {
        public static IServiceCollection AgregarInfraestructura(
            this IServiceCollection services,
            IConfiguration config)
        {
            // ── Base de datos MySQL — DbContextPool como Singleton administrado ──
            var conn = config.GetConnectionString("MySQL");
            services.AddDbContextPool<RepositorioBD>(opt =>
                opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

            // ── Repositorios (Scoped) ────────────────────────────────────────
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
         

            // ── Hasher BCrypt (Scoped) ───────────────────────────────────────
            // Reutilizado de Servicio_Clientes/HasherSimple — misma lógica, work factor 12
            services.AddScoped<IContraHasher, BcryptHasher>();

            // ── Factory Method (Scoped — necesita repositorio y hasher) ─────
            services.AddScoped<IUsuarioFabrica, FabricaUsuario>();

            // ── JWT: Singleton con Lazy interno ─────────────────────────────
            // Reutilizado de Servicio_Clientes/ServicioToken — migrado a Singleton Lazy
            services.Configure<JwtSettings>(config.GetSection(JwtSettings.Seccion));
            services.AddSingleton<IJwtServicio, JwtServicio>();

            // ── Email: reutilizado de Servicio_Clientes/ServicioEmail ────────
            // Cambio: IConfiguration → IOptions[EmailSettings]
            services.Configure<EmailSettings>(config.GetSection(EmailSettings.Seccion));
            services.AddScoped<IEmailServicio, EmailServicio>();

            // ── Casos de uso (Scoped) ────────────────────────────────────────
            // En Servicio_Clientes todos vivían en UsuarioServicio (un único servicio)
            // Aquí cada caso de uso tiene una sola responsabilidad (SRP)
            services.AddScoped<InicioSesionUsuarioCasoDeUso>();
            services.AddScoped<CrearUsuarioCasoDeUso>();
            services.AddScoped<CambiarContraCasoDeUso>();
            services.AddScoped<ObtenerUsuariosCasoDeUso>();
            services.AddScoped<ActualizarUsuarioCasoDeUso>();

            return services;
        }
    }

}
