using MicroServicioUsuarios.Infraestructura;
using MicroServicioUsuarios.Infraestructura.ConexionBD;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Infraestructura (DI, EF Core, Repositorios, Casos de Uso) ────────
builder.Services.AgregarInfraestructura(builder.Configuration);

// ── CORS — permite que el Frontend Razor se comunique con este MS ─────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001",
                "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Autenticación JWT ─────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Emisor,
            ValidAudience = jwtSettings.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.ClaveSecreta)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── Inicializar las tablas requeridas por el microservicio ──────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RepositorioBD>();
    await db.InicializarEsquemaAsync();
}

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
