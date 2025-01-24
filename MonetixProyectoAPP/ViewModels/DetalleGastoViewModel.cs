using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Globalization;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace MonetixProyectoAPP.ViewModels
{
    public class DetalleGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly ApiPublicaService _apiPublicaService = new();

        private GastoResponse _gasto;
        public GastoResponse Gasto
        {
            get => _gasto;
            set
            {
                SetProperty(ref _gasto, value);
                OnPropertyChanged(nameof(ValorPendiente));
            }
        }

        public double ValorPendiente => Gasto?.Valor - Gasto?.ValorPagado ?? 0;

        public ICommand EliminarGastoCommand { get; }
        public ICommand PagarGastoCommand { get; }

        public DetalleGastoViewModel(GastoService gastoService, int gastoId)
        {
            _gastoService = gastoService;
            EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
            PagarGastoCommand = new Command(async () => await PagarGastoAsync());
            CargarGasto(gastoId);
        }

        private async Task CargarGasto(int gastoId)
        {
            await ExecuteAsync(async () =>
            {
                Gasto = await _gastoService.GetGastoByIdAsync(gastoId);
            });
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

            if (double.TryParse(valorPagado, out double valor))
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
                        try
                        {
                            await _gastoService.PagarGastoAsync(Gasto.IdGasto, valor);
                            await Application.Current.MainPage.DisplayAlert("Éxito",
                                "El pago ha sido procesado correctamente", "OK");
                            await CargarGasto(Gasto.IdGasto);
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error",
                                $"Error al procesar el pago: {ex.Message}", "OK");
                        }
                    });
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "El valor ingresado no es válido", "OK");
            }
        }
    }
}