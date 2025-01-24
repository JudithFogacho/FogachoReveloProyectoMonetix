using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos
builder.Services.AddDbContext<FogachoReveloDataBase>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FogachoReveloDataBase")
        ?? throw new InvalidOperationException("Connection string 'FogachoReveloDataBase' not found.")));

// Configuración de servicios
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();