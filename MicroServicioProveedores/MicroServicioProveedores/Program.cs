using MicroservicioProveedores.Infraestructura.Persistence;
using MicroservicioProveedores.Infraestructura.Persistence.FactoriaCreadores;
using MicroservicioProveedores.Infraestructura.ProductosConcretos;
using MicroServicioProveedores.Aplicacion.CasosDeUso;
using MicroServicioProveedores.Aplicacion.Validadores;
using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var bd = RepositorioBD.Instancia;

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");

var connectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("La configuración de MongoDB (ConnectionString o DatabaseName) no está presente en appsettings.json");
}

builder.Services.AddScoped<IRepositorio<Proveedor>>(provider => {
    return new CreadorProveedorRepositorio().CrearRepositorio();
});

bd.Initiate(connectionString, databaseName);

//Validadores
builder.Services.AddScoped<ProveedorValidador>();

//Casos De Uso
builder.Services.AddScoped<CasoDeUsoCrearProveedor>();
builder.Services.AddScoped<CasoDeUsoObtenerProveedor>();
builder.Services.AddScoped<CasoDeUsoActualizarProveedor>();
builder.Services.AddScoped<CasoDeUsoEliminarProveedor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
