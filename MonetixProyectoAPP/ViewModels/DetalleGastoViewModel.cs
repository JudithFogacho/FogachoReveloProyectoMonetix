using Azure;
using Microsoft.EntityFrameworkCore.Query;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonetixProyectoAPP.ViewModels
{
    public class DetalleGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        private Gasto _gasto;

        public Gasto Gasto
        {
            get => _gasto;
            set
            {
                SetProperty(ref _gasto, value);
                OnPropertyChanged(nameof(ValorPendiente));
                OnPropertyChanged(nameof(Estado));
            }
        }

        public double ValorPendiente => Gasto?.Valor.GetValueOrDefault() - Gasto?.ValorPagado ?? 0;

        public string Estado
        {
            get
            {
                if (ValorPendiente == 0)
                {
                    return "Finalizado";
                }
                else if (Gasto.FechaFinal < DateTime.Now)
                {
                    return "Atrasado";
                }
                else
                {
                    return "Pendiente";
                }
            }
        }

        public ICommand EliminarGastoCommand { get; }
        public ICommand PagarGastoCommand { get; }

        public DetalleGastoViewModel(Gasto gasto)
        {
            Gasto = gasto;
            EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
            PagarGastoCommand = new Command(async () => await PagarGastoAsync());

        }

        private async Task EliminarGastoAsync()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar Eliminación",
                    "¿Está seguro de eliminar este gasto?",
                    "Sí",
                    "No"
                );
            if (confirmar)
            {
                await ExecuteAsync(async () =>
                {
                    await _gastoService.DeleteGastoAsync(Gasto.IdGasto);
                    await Shell.Current.GoToAsync("///PaginaInicial");
                });
            }
        }

        private async Task PagarGastoAsync()
        {
            var valorPagado = await Application.Current.MainPage.DisplayPromptAsync(
                    "Pagar Gasto",
                    "Ingrese el valor a pagar",
                    "Pagar",
                    "Cancelar",
                    keyboard: Keyboard.Numeric
                );

            if (TryParseValor(valorPagado, out double valor))
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar Pago",
                    $"¿Está seguro de pagar {valor:C2}?",
                    "Sí",
                    "No"
                    );

                if (confirmar)
                {
                    await ExecuteAsync(async () =>
                    {
                        var pagoData = new
                        {
                            ValorPagado = valor
                        };
                        var url = $"Gasto/Pagar/{Gasto.IdGasto}";
                        var content = new StringContent(JsonConvert.SerializeObject(pagoData), Encoding.UTF8, "application/json");
                        var response = await _httpClient.PutAsync(url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            await Application.Current.MainPage.DisplayAlert("Ëxito", "El pago ha sido procesado correctamente", "OK");
                            await Shell.Current.GoToAsync("//PaginaInicial");
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
                        }
                    });
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El valor ingresado no es válido", "OK");
            }
        }
        private bool TryParseValor(string input, out double valor)
        {
            input = input?.Replace("$", "").Replace(",", "").Trim();
            return double.TryParse(input, NumberStyles.Currency, CultureInfo.InvariantCulture, out valor);
        }
    }
}