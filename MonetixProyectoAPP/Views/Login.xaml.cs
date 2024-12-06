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
        // Verifica que los campos no est�n vac�os
        if (string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese email y contrase�a", "OK");
            return;
        }

        var credentials = new
        {
            Email = txtEmail.Text?.Trim(), // Aseg�rate de que estos nombres coincidan con tus Entry en el XAML
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

                await DisplayAlert("�xito", "Inicio de sesi�n correcto", "OK");
                // Navega a la p�gina inicial
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
            await DisplayAlert("Error de Conexi�n",
                "No se pudo conectar con el servidor. Verifique su conexi�n a internet.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                "Ocurri� un error inesperado. Por favor intente nuevamente.", "OK");
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