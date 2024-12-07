namespace MonetixProyectoAPP.Views
{
    using MonetixProyectoAPP.Models;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;

    public partial class DetalleGasto : ContentPage
    {
        private readonly int _gastoId;

        private Gasto _gasto;

        public DetalleGasto(Gasto gasto)
        {
            InitializeComponent();
            _gasto = gasto;
            BindingContext = _gasto; // Asocia el gasto al BindingContext
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Estás seguro de eliminar este gasto?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    string url = $"https://tuapi.com/api/gastos/{_gastoId}"; // Cambia por tu URL base
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "El gasto ha sido eliminado.", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el gasto.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                }
            }
        }

        private void OnPagarGastoClicked(object sender, EventArgs e)
        {
            // Mostrar el diálogo de pago
            PagoDialogo.IsVisible = true;
        }

        private void OnCancelarPagoClicked(object sender, EventArgs e)
        {
            // Ocultar el diálogo de pago
            PagoDialogo.IsVisible = false;
        }

        private async void OnConfirmarPagoClicked(object sender, EventArgs e)
        {
            var valorPago = EntryPago.Text;
            if (double.TryParse(valorPago, out double cantidad))
            {
                try
                {
                    // Aquí puedes agregar la lógica para registrar el pago
                    // Por ejemplo: Llamar a la API para registrar el pago
                    HttpClient client = new HttpClient();
                    string url = $"https://tuapi.com/api/gastos/pagar/{_gastoId}";
                    var response = await client.PutAsync(url, new StringContent(cantidad.ToString()));

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Pago realizado correctamente.", "OK");
                        PagoDialogo.IsVisible = false;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un problema al procesar el pago.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Por favor ingresa un valor válido para el pago.", "OK");
            }
        }
    }
}
