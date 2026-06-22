using MicroServicioReportes.Aplicacion.Builders;
using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Aplicacion.Prototipos;
using MicroServicioReportes.Aplicacion.Servicios;
using MicroServicioReportes.Dominio.Interfaces;
using MicroServicioReportes.Infraestructura;
using MicroServicioReportes.Infraestructura.Generadores;
using MicroServicioReportes.Infraestructura.Repositorios;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<IReporteBuilder, ReporteDocumentoBuilder>();
builder.Services.AddSingleton<IPlantillaReporteProveedor, PlantillaReporteProveedor>();
builder.Services.AddScoped<IReporteServicio, ReporteServicio>();
builder.Services.AddSingleton<IGeneradorReporte, PdfGeneradorReporte>();

var serviciosExternos = builder.Configuration.GetSection("ServiciosExternos");
var ventasUrl = serviciosExternos["VentasUrl"] ?? "http://localhost:5036/";
var productosUrl = serviciosExternos["ProductosUrl"] ?? "http://localhost:5038/";

if (serviciosExternos.GetValue<bool>("UsarRepositorioEnMemoria"))
{
    builder.Services.AddSingleton<IReporteRepositorio, ReporteRepositorioEnMemoria>();
}
else
{
    builder.Services.AddScoped<IReporteRepositorio>(_ => new ReporteRepositorioHttp(
        new HttpClient { BaseAddress = new Uri(ventasUrl) },
        new HttpClient { BaseAddress = new Uri(productosUrl) }));
}

builder.Services.AddSingleton<IBitacoraReporteRepositorio, BitacoraReporteRepositorioEnMemoria>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
<<<<<<< HEAD
builder.Services.AddInfraestructura(builder.Configuration);

QuestPDF.Settings.License = LicenseType.Community;
=======
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
>>>>>>> 11d1d7923b9809be2802c894f952feb1018e4427

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroServicio Reportes API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
