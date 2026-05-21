using MicroservicioProveedores.Infraestructura.Persistence;
using MicroservicioProveedores.Infraestructura.Persistence.FactoriaCreadores;
using MicroservicioProveedores.Infraestructura.ProductosConcretos;
using MicroServicioProveedores.Aplicacion.Validadores;
using MicroServicioProveedores.Dominio.Interfaces;
using MicroServicioProveedores.Dominio.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

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

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
