using MicroServicioReportes.Aplicacion.Builders;
using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Aplicacion.Prototipos;
using MicroServicioReportes.Aplicacion.Servicios;
using MicroServicioReportes.Dominio.Interfaces;
using MicroServicioReportes.Infraestructura.Generadores;
using MicroServicioReportes.Infraestructura.Repositorios;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configurar QuestPDF con licencia Community
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<IReporteBuilder, ReporteDocumentoBuilder>();
builder.Services.AddSingleton<IPlantillaReporteProveedor, PlantillaReporteProveedor>();
builder.Services.AddScoped<IReporteServicio, ReporteServicio>();
builder.Services.AddSingleton<IGeneradorReporte, PdfGeneradorReporte>();
builder.Services.AddSingleton<IReporteRepositorio, ReporteRepositorioEnMemoria>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
