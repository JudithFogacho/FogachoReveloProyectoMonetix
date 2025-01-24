using System.Text.Json.Serialization;
using MonetixFogachoReveloAPI.Data;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using MonetixFogachoReveloAPI.Data.Models;
using MonetixFogachoReveloAPI.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos
builder.Services.AddSqlServer<FogachoReveloDataContext>(
    builder.Configuration.GetConnectionString("FogachoReveloDataBase"));

// Configuración de controladores
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MonetixFogachoReveloAPI",
        Version = "v1",
        Description = "API para gestión de gastos personales"
    });

    c.SchemaFilter<EnumSchemaFilter>();

    // Eliminada la configuración de seguridad JWT
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonetixFogachoReveloAPI v1");
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();

// Eliminados middlewares de autenticación JWT
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();
app.MapUsuarioEndpoints();
app.MapGastoEndpoints();

app.Run();