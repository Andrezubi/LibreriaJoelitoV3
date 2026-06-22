using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Aplicacion.Servicios;
using MicroServicioReportes.Infraestructura.Generadores;
using MicroServicioReportes.Infraestructura.Mensajeria.Consumers;
using MicroServicioReportes.Infraestructura.Mensajeria.Rabbit;
using MicroServicioReportes.Infraestructura.Persistencia;
using MicroServicioReportes.Infraestructura.Repositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroServicioReportes.Infraestructura
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraestructura(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("MySqlReportes");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No se encontró la cadena de conexión 'MySqlReportes'.");

            services.AddSingleton(provider =>
            {
                var repositorioBD = RepositorioBD.Instancia;
                repositorioBD.Initiate(connectionString);
                return repositorioBD;
            });

            services.Configure<RabbitMqOptions>(
                configuration.GetSection("RabbitMQ"));

            services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
            services.AddScoped<IComprobanteVentaRepositorio, ComprobanteVentaRepositorio>();
            services.AddScoped<IProcessedMessageRepositorio, ProcessedMessageRepositorio>();

            services.AddScoped<IComprobanteVentaSagaServicio, ComprobanteVentaSagaServicio>();

            services.AddScoped<IComprobanteVentaPdfServicio, ComprobanteVentaPdfServicio>();

            services.AddHostedService<ReportesSagaConsumerService>();

            return services;
        }
    }
}