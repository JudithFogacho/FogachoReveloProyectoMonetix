using Microsoft.Maui.Controls;

namespace MonetixProyectoAPP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Captura excepciones no manejadas
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            MainPage = new AppShell();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Muestra el error en un diálogo de alerta
            if (e.ExceptionObject is Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert("Error", $"Ocurrió un error inesperado: {ex.Message}", "OK");
                });
            }
        }
    }
}