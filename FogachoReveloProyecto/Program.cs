using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración básica de Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "DataProtection-Keys")))
    .SetApplicationName("Monetix");

// 2. Configuración esencial de cookies
builder.Services.Configure<CookiePolicyOptions>(options => {
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});

// 3. Configuración de base de datos
builder.Services.AddDbContext<FogachoReveloDataBase>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FogachoReveloDataBase")));

// 4. Configuración básica de antiforgery
builder.Services.AddAntiforgery(options => {
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// 5. Configuración MVC básica
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Configuración del pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthorization();

// 7. Endpoints esenciales
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();