using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using AlmaHogarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("SpringApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:SpringApi"]
    );
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
});

builder.Services.AddHttpClient("DotNetApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:DotNetApi"]
    );
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
});

// Registrar servicios
builder.Services.AddScoped<IPromocionService, PromocionService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // necesario para vistas y assets

app.UseRouting();

app.UseSession();       // SOLO UNA VEZ y después de UseRouting

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();