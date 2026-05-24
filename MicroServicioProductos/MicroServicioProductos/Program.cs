using MicroServicioProductos.Aplicacion.Servicios;
using MicroServicioProductos.Dominio.Validadores;
using MicroServicioProductos.Infraestructura.FactoriaCreadores;
using MicroServicioProductos.Infraestructura.Persistencia;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<ProductoRepositorio>(provider => {
    return new ProductoCreadorRepositorio().CrearRepositorio();
});
builder.Services.AddScoped<PresentacionRepositorio>(provider => {
    return new PresentacionCreadorRepositorio().CrearRepositorio();
});
builder.Services.AddScoped<ProductoRepositorio>(provider => {
    return new ProductoCreadorRepositorio().CrearRepositorio();
});
builder.Services.AddScoped<PresentacionProductoRepositorio>(provider => {
    return new PresentacionProductoCreadorRepositorio().CrearRepositorio();
});
builder.Services.AddScoped<MarcaRepositorio>(provider => {
    return new MarcaCreadorRepositorio().CrearRepositorio();
});

builder.Services.AddScoped<BitacoraRepositorio>();

builder.Services.AddScoped<PresentacionServicio>();
builder.Services.AddScoped<ProductoServicio>();
builder.Services.AddScoped<MarcaServicio>();



builder.Services.AddScoped<ProductoValidador>();
builder.Services.AddScoped<MarcaValidador>();


var app = builder.Build();

// Config BD
var bd = RepositorioBD.Instancia;

var connectionString = builder.Configuration.GetConnectionString("ConnectionSqlServer");
bd.Initiate(connectionString);

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
