using MicroservicioProveedores.Infraestructura.Persistence;

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

bd.Initiate(connectionString, databaseName);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
