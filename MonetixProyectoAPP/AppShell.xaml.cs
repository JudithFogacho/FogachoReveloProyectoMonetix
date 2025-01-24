using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Views;

namespace MonetixProyectoAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar las rutas de navegación
            Routing.RegisterRoute("Login", typeof(Views.Login));
            Routing.RegisterRoute("Registro", typeof(Views.Registro));
            Routing.RegisterRoute("PaginaInicial", typeof(Views.PaginaInicial));
            Routing.RegisterRoute("IngresarGasto", typeof(Views.IngresarGasto));
            Routing.RegisterRoute("DetalleGasto", typeof(Views.DetalleGasto));
            Routing.RegisterRoute("TiendasFavoritas", typeof(Views.TiendasFavoritas)); // Nueva ruta
            Routing.RegisterRoute("TiendasFavoritasGuardadas", typeof(Views.TiendasFavoritasGuardadas)); // Nueva ruta
        }
    }
}