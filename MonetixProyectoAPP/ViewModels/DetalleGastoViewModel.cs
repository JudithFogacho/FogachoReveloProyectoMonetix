using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.ViewModels;

public class DetalleGastoViewModel : BaseViewModel
{
    private readonly GastoService _gastoService;
    private GastoResponse _gasto;
    private bool _isLoading;

    public GastoResponse Gasto
    {
        get => _gasto;
        set
        {
            SetProperty(ref _gasto, value);
            OnPropertyChanged(nameof(ValorPendiente));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public double ValorPendiente => Gasto?.Valor - Gasto?.ValorPagado ?? 0;

    public ICommand PagarGastoCommand { get; }

    public DetalleGastoViewModel(GastoService gastoService, int gastoId)
    {
        _gastoService = gastoService;
        PagarGastoCommand = new Command(async () => await PagarGastoAsync());

        Task.Run(async () => await CargarGasto(gastoId));
    }

    private async Task CargarGasto(int gastoId)
    {
        try
        {
            IsLoading = true;
            var gastos = await _gastoService.GetGastosAsync();
            Gasto = gastos.FirstOrDefault(g => g.IdGasto == gastoId);

            if (Gasto == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se encontró el gasto",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al cargar el gasto: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PagarGastoAsync()
    {
        var valorPagado = await Application.Current.MainPage.DisplayPromptAsync(
            "Pagar Gasto",
            "Ingrese el valor a pagar",
            "Pagar",
            "Cancelar",
            keyboard: Keyboard.Numeric);

        if (!double.TryParse(valorPagado, out double valor))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "El valor ingresado no es válido",
                "OK");
            return;
        }

        var confirmar = await Application.Current.MainPage.DisplayAlert(
            "Confirmar Pago",
            $"¿Está seguro de pagar {valor:C2}?",
            "Sí",
            "No");

        if (!confirmar) return;

        try
        {
            IsLoading = true;
            var resultado = await _gastoService.PagarGastoAsync(Gasto.IdGasto, valor);

            if (resultado != null)
            {
                Gasto = resultado;
                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Pago procesado correctamente",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se pudo procesar el pago",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error al procesar el pago: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}