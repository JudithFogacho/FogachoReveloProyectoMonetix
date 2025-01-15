using Azure;
using Microsoft.EntityFrameworkCore.Query;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonetixProyectoAPP.ViewModels
{
    public class DetalleGastoViewModel: BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();

        private Gasto _gasto;

        public Gasto Gasto {
            get => _gasto;
            set {
                SetProperty(ref _gasto, value);
                OnPropertyChanged(nameof(ValorPendiente));
                OnPropertyChanged(nameof(Estado));
            }
        }

        public double ValorPendiente => Gasto?.Valor.GetValueOrDefault() - Gasto?.ValorPagado ?? 0;

        public string Estado {
            get {
                if (ValorPendiente == 0)
                {
                    return "Pagado";
                }
                else if (Gasto.FechaFinal < DateTime.Now)
                {
                    return "Atrasado";
                }
                else {
                    return "Finalizado";
                }
            }
        }

        public ICommand EliminarGastoCommand { get; }
        public ICommand PagarGastoCommand { get; }

        public DetalleGastoViewModel(Gasto gasto) { 
            Gasto = gasto;
            EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
            PagarGastoCommand = new Command(async () => await PagarGastoAsync());

        }

        private async Task EliminarGastoAsync()
        {
            await ExecuteAsync(async () =>
            {
                await _gastoService.DeleteGastoAsync(Gasto.IdGasto);
                await Shell.Current.GoToAsync("///PaginaInicial");
            }); 
        }

        private async Task PagarGastoAsync()
        {
            await ExecuteAsync(async () =>
            {
                var valorPagado = await Application.Current.MainPage.DisplayPromptAsync("Pagar Gasto", "Ingrese el valor a pagar","Pagar", "Cancelar", keyboard: Keyboard.Numeric);
                if (double.TryParse(valorPagado, out double valor)) {
                    var pagoData = new
                    {
                        IdGasto = Gasto.IdGasto,
                        ValorPagado = valor
                    };

                    var response = await _gastoService.PagarGastoAsync(pagoData);

                    if (response.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Exit", "El pago ha sido procesado correctamente", "Ok");
                        await Shell.Current.GoToAsync("///PaginaInicial");
                    }
                    else {
                        await Application.Current.MainPage.DisplayAlert("Error", "El valor ingresado no es valido", "OK");
                    }

                }
            });
        }
    }
}
