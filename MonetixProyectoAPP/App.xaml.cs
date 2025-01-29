using Microsoft.Maui.Controls;

namespace MonetixProyectoAPP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Registrar manejadores de excepciones
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if WINDOWS
            Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) =>
            {
                args.Handled = true;
                ShowError(args.Exception);
            };
#endif

            MainPage = new AppShell();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                ShowError(exception);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            if (e.Exception != null)
            {
                ShowError(e.Exception);
            }
        }

        private void ShowError(Exception exception)
        {
            try
            {
                // Obtener la excepción más interna
                var innermost = GetInnermostException(exception);

                // Registrar el error
                System.Diagnostics.Debug.WriteLine($"Error no manejado: {innermost.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {innermost.StackTrace}");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Mostrar un mensaje de error amigable al usuario
                        await Shell.Current.DisplayAlert(
                            "Error",
                            GetUserFriendlyMessage(innermost),
                            "OK");
                    }
                    catch
                    {
                        // Si falla el DisplayAlert, intentar con un mensaje más simple
                        Application.Current?.MainPage?.DisplayAlert(
                            "Error",
                            "Ha ocurrido un error inesperado en la aplicación.",
                            "OK");
                    }
                });
            }
            catch
            {
                // Último recurso si todo lo demás falla
                System.Diagnostics.Debug.WriteLine("Error crítico al manejar una excepción no controlada");
            }
        }

        private Exception GetInnermostException(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        private string GetUserFriendlyMessage(Exception ex)
        {
            // Personalizar mensajes según el tipo de excepción
            return ex switch
            {
                HttpRequestException _ => "No se pudo conectar al servidor. Por favor, verifica tu conexión a internet.",
                UnauthorizedAccessException _ => "No tienes permiso para realizar esta acción.",
                ArgumentException _ => "Datos incorrectos o inválidos.",
                NullReferenceException _ => "Se encontró un error en los datos de la aplicación.",
                TimeoutException _ => "La operación ha tardado demasiado tiempo. Por favor, inténtalo de nuevo.",
                InvalidOperationException _ => "La operación no pudo completarse. Por favor, inténtalo de nuevo.",
                // Agregar más casos según sea necesario
                _ => "Ha ocurrido un error inesperado en la aplicación."
            };
        }
    }
}