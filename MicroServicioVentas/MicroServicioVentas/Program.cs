using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Aplicacion.Servicios;
using MicroServicioVentas.Infraestructura.FactoriaCreadores;
using MicroServicioVentas.Infraestructura.Mensajeria.Consumers;
using MicroServicioVentas.Infraestructura.Mensajeria.Outbox;
using MicroServicioVentas.Infraestructura.Mensajeria.Rabbit;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;
using MicroServicioVentas.Infraestructura.ServiciosExternos;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("MySqlVentas");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No se encontró la cadena de conexión MySqlVentas.");
}

RepositorioBD.Instancia.Initiate(connectionString);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ")
);

// Repositorios
builder.Services.AddSingleton<VentaRepositorio>(_ =>
    new VentaCreadorRepositorio().CrearRepositorio());

builder.Services.AddSingleton<DetalleVentaRepositorio>(_ =>
    new DetalleVentaCreadorRepositorio().CrearRepositorio());

builder.Services.AddSingleton<VentaClienteSnapshotRepositorio>(_ =>
    new VentaClienteSnapshotCreadorRepositorio().CrearRepositorio());

builder.Services.AddSingleton<OutboxMessageRepositorio>(_ =>
    new OutboxMessageCreadorRepositorio().CrearRepositorio());

builder.Services.AddSingleton<ProcessedMessageRepositorio>(_ =>
    new ProcessedMessageCreadorRepositorio().CrearRepositorio());

// Infraestructura
builder.Services.AddSingleton<RabbitPublisher>();
builder.Services.AddScoped<IPdfServicio, PdfServicio>();

// Servicios de aplicación
builder.Services.AddScoped<FachadaRealizarVenta>();
builder.Services.AddScoped<FachadaAnularVenta>();
builder.Services.AddScoped<FachadaGestionInventario>();
builder.Services.AddScoped<ConsultaVentaServicio>();

builder.Services.AddSingleton<VentaSagaServicio>();

// Servicios en segundo plano
builder.Services.AddHostedService<OutboxPublisherService>();
builder.Services.AddHostedService<VentasSagaConsumerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();