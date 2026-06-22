using FrontendLibreria.Adaptadores;
using FrontendLibreria.Adaptadores.Cliente;
using FrontendLibreria.Adaptadores.Marca;
using FrontendLibreria.Adaptadores.Producto;
using FrontendLibreria.Adaptadores.ProveedoresAdapter;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// HttpClient para consumir el microservicio de usuarios.
var uriUsuarios = builder.Configuration["ApiSettings:MicroServicioUsuariosUrl"];
builder.Services.AddHttpClient<IUsuarioServicioAdapter, UsuarioServicioAdapter>(client =>
{
    client.BaseAddress = new Uri(uriUsuarios!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// En desarrollo, permitir certificados autofirmados
if (builder.Environment.IsDevelopment())
{
    // HttpClientHandler configurado globalmente para ignorar errores de certificado
    var httpClientHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    // Registrar un named HttpClient para desarrollo
    builder.Services.AddHttpClient("Development")
        .ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);
}

// HttpClient para consumir los microservicios integrados desde main.
builder.Services.AddHttpClient<IAdaptadorProducto, AdaptadorProducto>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:MicroServicioProductosUrl"] ?? "http://localhost:5038"
    );
});

builder.Services.AddHttpClient<IAdaptadorMarca, AdaptadorMarca>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:MicroServicioProductosUrl"] ?? "http://localhost:5038"
    );
});

builder.Services.AddHttpClient<IProveedorAdapter, ProveedorAdapter>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:MicroServicioProveedoresUrl"]!);
});

builder.Services.AddHttpClient<IAdaptadorCliente, AdaptadorCliente>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:MicroServicioClientesUrl"]!);
});

// Configurar Autenticación por Cookies para Razor Pages
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Error";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
