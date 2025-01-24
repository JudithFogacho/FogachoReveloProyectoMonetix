using Microsoft.Extensions.Logging;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;
using MonetixProyectoAPP.Views;
using System;

namespace MonetixProyectoAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Koulen-Regular.ttf", "Koulen");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registro de Servicios
            builder.Services.AddSingleton<UsuarioService>();
            builder.Services.AddSingleton<GastoService>();
            builder.Services.AddSingleton<ApiPublicaService>();

            // Registro de SQLiteService con la ruta de la base de datos
            builder.Services.AddSingleton<SQLiteService>(serviceProvider =>
            {
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tiendas.db");
                return new SQLiteService(dbPath);
            });

            // Registro de Páginas
            builder.Services.AddTransient<Login>();
            builder.Services.AddTransient<Registro>();
            builder.Services.AddTransient<PaginaInicial>();
            builder.Services.AddTransient<IngresarGasto>();
            builder.Services.AddTransient<DetalleGasto>();
            builder.Services.AddTransient<TiendasFavoritas>(); // Página para añadir tiendas
            builder.Services.AddTransient<TiendasFavoritasGuardadas>(); // Nueva página para ver tiendas guardadas
            builder.Services.AddSingleton<AppShell>();

            // Registro de ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistroViewModel>();
            builder.Services.AddTransient<PaginaInicialViewModel>();
            builder.Services.AddTransient<IngresarGastoViewModel>();
            builder.Services.AddTransient<DetalleGastoViewModel>();
            builder.Services.AddTransient<TiendasFavoritasViewModel>(); // ViewModel para añadir tiendas
            builder.Services.AddTransient<TiendasFavoritasGuardadasViewModel>(); // Nuevo ViewModel para tiendas guardadas

            // Configuración adicional
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}