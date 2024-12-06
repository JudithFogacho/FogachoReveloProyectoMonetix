using System.Net.Http.Json;
using MonetixProyectoAPP.Models;
namespace MonetixProyectoAPP.Views;


public partial class Login : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7156/api/")
    };

    public Login()
    {
        InitializeComponent();
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Verifica que los campos no estén vacíos
        if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese email y contraseña", "OK");
            return;
        }

        var credentials = new
        {
            Email = txtEmail.Text?.Trim(), // Asegúrate de que estos nombres coincidan con tus Entry en el XAML
            Password = txtPassword.Text?.Trim() // Cambiado a Password para coincidir con tu modelo de Usuario
        };

        try
        {
            // Muestra un indicador de carga
            IsBusy = true;

            var response = await _httpClient.PostAsJsonAsync("Usuario/login", credentials);

            if (response.IsSuccessStatusCode)
            {
                var usuario = await response.Content.ReadFromJsonAsync<Usuario>();

                // Guarda los datos del usuario si es necesario
                // Preferences.Set("UserId", usuario.IdUsuario);
                // Preferences.Set("UserEmail", usuario.Email);

                await DisplayAlert("Éxito", "Inicio de sesión correcto", "OK");
                // Navega a la página inicial
                await Shell.Current.GoToAsync("///PaginaInicial");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", errorMessage, "OK");
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Error de Conexión",
                "No se pudo conectar con el servidor. Verifique su conexión a internet.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                "Ocurrió un error inesperado. Por favor intente nuevamente.", "OK");
        }
        finally
        {
            // Oculta el indicador de carga
            IsBusy = false;
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("Registro");
    }
}