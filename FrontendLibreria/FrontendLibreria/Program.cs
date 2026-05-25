using FrontendLibreria.Adaptadores.ProveedoresAdapter;

using FrontendLibreria.Adaptadores;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();


// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient<IProveedorAdapter, ProveedorAdapter>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:MicroServicioProveedoresUrl"]!);
});

builder.Services.AddHttpClient<IAdaptadorCliente, AdaptadorCliente>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:MicroServicioClientesUrl"]!);
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

//app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
