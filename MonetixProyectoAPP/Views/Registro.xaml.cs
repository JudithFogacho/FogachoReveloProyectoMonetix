using System.Net.Http.Json;

namespace MonetixProyectoAPP.Views;

public partial class Registro : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7156/api/") 
    };
    public Registro()
	{
		InitializeComponent();
	}

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // Crear el objeto con los datos del usuario
        var nuevoUser = new
        {
            Nombre = "NombreDeUsuario", // Cambia según tus campos de entrada
            Email = "correo@example.com", // Asegúrate de obtener estos datos del usuario
            Contrasena = "Contrasena123"  // Puedes aplicar hashing en la API
        };

        try
        {
            // Enviar datos a la API
            var response = await _httpClient.PostAsJsonAsync("usuarios/registro", nuevoUser);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Registro exitoso", "Usuario registrado correctamente.", "OK");
                await Navigation.PushAsync(new Login());
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Error al registrar: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo registrar al usuario: {ex.Message}", "OK");
        }
    }
}