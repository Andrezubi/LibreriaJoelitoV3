using MicroServicioClientes.Aplicacion.Servicios;
using MicroServicioClientes.Dominio.Validadores;
using MicroServicioClientes.Infrestructura.FactoriaCreadores;
using MicroServicioClientes.Infrestructura.Persistencia;
using MicroServicioClientes.Infrestructura.Persistencia.FactoriaProductos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();


// Add services to the container.

builder.Services.AddScoped<ClienteRepositorio>(provider => {
    return new ClienteCreadorRepositorio().CrearRepositorio();
});
builder.Services.AddScoped<BitacoraRepositorio>();

builder.Services.AddScoped<ClienteServicio>();

builder.Services.AddScoped<ClienteValidador>();


builder.Services.AddControllers();




var app = builder.Build();

//CONFIG BD
var bd = RepositorioBD.Instancia;

// select connection string from appsettings
var connectionString = builder.Configuration.GetConnectionString("ConnectionMySql");
bd.Initiate(connectionString);
/*
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Servicio Ventas API V1");
        c.RoutePrefix = "swagger"; // optional but explicit
    });
}
*/
app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();
