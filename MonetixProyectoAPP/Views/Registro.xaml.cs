using System.Net.Http.Json;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Views
{
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
            // Verifica que los campos no estén vacíos
            if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtApellido.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                await DisplayAlert("Error", "Por favor complete todos los campos", "OK");
                return;
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = txtNombre.Text?.Trim(),
                Apellido = txtApellido.Text?.Trim(),
                Email = txtEmail.Text?.Trim(),
                Password = txtPassword.Text?.Trim()
            };

            try
            {
                // Muestra un indicador de carga
                IsBusy = true;

                var response = await _httpClient.PostAsJsonAsync("Usuario", nuevoUsuario);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioCreado = await response.Content.ReadFromJsonAsync<Usuario>();
                    await DisplayAlert("Éxito", "Registro completado con éxito", "OK");
                    await Shell.Current.GoToAsync("///Login");
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

        private async void OnLoginTapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("Login");
        }
    }
}
