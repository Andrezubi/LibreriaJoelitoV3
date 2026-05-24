using FrontendLibreria.Adaptadores.Marca;
using FrontendLibreria.Adaptadores.Producto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorPages();

// HttpClient para consumir el microservicio de productos
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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
