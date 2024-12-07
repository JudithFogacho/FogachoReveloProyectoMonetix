namespace MonetixProyectoAPP.Views
{
    using MonetixProyectoAPP.Models;
    using System;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;
    using System.Text;
    using System.Net.Http.Json;

    public partial class DetalleGasto : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")  
        };

        private int _gastoId; 
        public DetalleGasto(Gasto gasto)
        {
            InitializeComponent();
            _gastoId = gasto.IdGasto; 
            BindingContext = gasto;
        }

        private void OnEliminarClicked(object sender, EventArgs e)
        {
            
            DisplayEliminarDialog();
        }

        // Mostrar cuadro de confirmación
        private void DisplayEliminarDialog()
        {
            EliminarDialogo.IsVisible = true;
        }

        private async void OnEliminarButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"gastos/{_gastoId}");

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Gasto eliminado correctamente.", "OK");
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
                await DisplayAlert("Error de Conexión", "No se pudo conectar con el servidor. Verifique su conexión a internet.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrió un error inesperado. Por favor intente nuevamente.", "OK");
            }
            finally
            {
                EliminarDialogo.IsVisible = false;
            }
        }

        // Este método se ejecuta cuando el usuario decide cancelar la eliminación
        private void OnCancelarButtonClicked(object sender, EventArgs e)
        {
            EliminarDialogo.IsVisible = false;
        }

        private async void OnPagarGastoClicked(object sender, EventArgs e)
        {
            // Mostrar el cuadro de pago
            PagoDialogo.IsVisible = true;
        }

        // Este método se ejecuta cuando el usuario cancela el pago
        private void OnCancelarPagoClicked(object sender, EventArgs e)
        {
            PagoDialogo.IsVisible = false;
        }

        private async void OnConfirmarPagoClicked(object sender, EventArgs e)
        {
            try
            {
                var pagoData = new
                {
                    IdGasto = _gastoId, // El ID del gasto que hemos asignado
                    ValorPagado = Convert.ToDouble(EntryPago.Text)
                };

                // Convertir el objeto a JSON
                var content = new StringContent(JsonConvert.SerializeObject(pagoData), Encoding.UTF8, "application/json");

                // Hacer la solicitud PUT para actualizar el gasto
                var response = await _httpClient.PutAsync($"Gasto/{_gastoId}", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "El pago ha sido procesado correctamente.", "OK");
                    PagoDialogo.IsVisible = false;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", errorMessage, "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Error de Conexión", "No se pudo conectar con el servidor. Verifique su conexión a internet.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrió un error inesperado. Por favor intente nuevamente.", "OK");
            }
        }
    }
}
