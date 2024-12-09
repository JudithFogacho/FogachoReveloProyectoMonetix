namespace MonetixProyectoAPP.Views
{
    using MonetixProyectoAPP.Models;
    using System;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;
    using System.Text;
    using System.IO;
    using System.Globalization;
    using System.Net.Http.Json;
    using System.Text.Json.Serialization;

    public partial class DetalleGasto : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        private int _gastoId;

        private Gasto _gasto;
        public Gasto Gasto
        {
            get => _gasto;
            set
            {
                _gasto = value;
                OnPropertyChanged(nameof(Gasto));
                OnPropertyChanged(nameof(ValorPendiente));
                OnPropertyChanged(nameof(Estado)); // Notificar cambios en la propiedad Estado
            }
        }

        public double ValorPendiente => Gasto?.Valor.GetValueOrDefault() - Gasto?.ValorPagado ?? 0;

        public string Estado
        {
            get
            {
                if (ValorPendiente == 0)
                    return "Pagado";
                else if (Gasto.FechaFinal < DateTime.Now)
                    return "Atrasado";
                else
                    return "Pendiente";
            }
        }

        public DetalleGasto(Gasto gasto)
        {
            InitializeComponent();
            _gastoId = gasto.IdGasto;
            Gasto = gasto;

            BindingContext = this;

        }

        private void OnEliminarClicked(object sender, EventArgs e)
        {
            DisplayEliminarDialog();
        }

        private void DisplayEliminarDialog()
        {
            EliminarDialogo.IsVisible = true;
        }

        private async void OnEliminarButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Gasto/{_gastoId}");

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Gasto eliminado correctamente.", "OK");
                    await Shell.Current.GoToAsync("///PaginaInicial");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al eliminar el gasto: {errorMessage}");
                    await DisplayAlert("Error", errorMessage, "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de Conexión: {ex.Message}");
                await DisplayAlert("Error de Conexión", "No se pudo conectar con el servidor. Verifique su conexión a internet.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                await DisplayAlert("Error", "Ocurrió un error inesperado. Por favor intente nuevamente.", "OK");
            }
            finally
            {
                EliminarDialogo.IsVisible = false;
            }
        }

        private void OnCancelarButtonClicked(object sender, EventArgs e)
        {
            EliminarDialogo.IsVisible = false;
        }

        private void OnPagarGastoClicked(object sender, EventArgs e)
        {
            PagoDialogo.IsVisible = true;
        }

        private void OnCancelarPagoClicked(object sender, EventArgs e)
        {
            PagoDialogo.IsVisible = false;
        }

        private async void OnConfirmarPagoClicked(object sender, EventArgs e)
        {
            try
            {
                var valorPagadoStr = EntryPago.Text.Replace("$", "").Replace(",", "").Trim();
                double valorPagado = double.Parse(valorPagadoStr, CultureInfo.InvariantCulture);

                var pagoData = new
                {
                    IdGasto = _gastoId,
                    ValorPagado = valorPagado
                };

                var content = new StringContent(JsonConvert.SerializeObject(pagoData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"Gasto/{_gastoId}", content);

                if (response.IsSuccessStatusCode)
                {
                    Gasto = await _httpClient.GetFromJsonAsync<Gasto>($"Gasto/{_gastoId}");

                    await DisplayAlert("Éxito", "El pago ha sido procesado correctamente.", "OK");
                    PagoDialogo.IsVisible = false;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al procesar el pago: {errorMessage}");
                    await DisplayAlert("Error", errorMessage, "OK");
                }
            }
            catch (FormatException)
            {
                await DisplayAlert("Error", "El valor ingresado no es válido. Por favor, ingresa un número válido.", "OK");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de Conexión: {ex.Message}");
                await DisplayAlert("Error de Conexión", "No se pudo conectar con el servidor. Verifique su conexión a internet.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                await DisplayAlert("Error", "Ocurrió un error inesperado. Por favor intente nuevamente.", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
