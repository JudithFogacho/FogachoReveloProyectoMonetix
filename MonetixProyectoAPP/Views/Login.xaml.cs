using System.Net.Http.Json;

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
        var credentials = new
        {
            Email = "correo@example.com", // Captura del campo de usuario
            Contrasena = "Contrasena123"  // Captura del campo de contraseña
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("usuarios/login", credentials);

            if (response.IsSuccessStatusCode)
            {
                // Ejemplo: Navegar al dashboard si el inicio de sesión es exitoso
                await DisplayAlert("Éxito", "Inicio de sesión correcto.", "OK");
                //await Navigation.PushAsync(new PaginaInicial());
            }
            else
            {
                await DisplayAlert("Error", "Usuario o contraseña incorrectos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo iniciar sesión: {ex.Message}", "OK");
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("Registro");
    }
}