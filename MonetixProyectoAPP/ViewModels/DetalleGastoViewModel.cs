using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using System.Globalization;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.ViewModels;

public class DetalleGastoViewModel : BaseViewModel
{
    private readonly GastoService _gastoService;
    private readonly int _gastoId;
    private Gasto _gasto;
    private bool _isLoading;
    private string _empresa = string.Empty;
    private string _descripcion = string.Empty;

    public Gasto Gasto
    {
        get => _gasto;
        set
        {
            if (SetProperty(ref _gasto, value))
            {
                OnPropertyChanged(nameof(ValorPendiente));
                OnPropertyChanged(nameof(EstaFinalizado));
                OnPropertyChanged(nameof(CategoriaTexto));
                OnPropertyChanged(nameof(EstadoTexto));
                OnPropertyChanged(nameof(ColorEstado));
                if (value != null)
                {
                    ExtraerEmpresaYDescripcion(value.Descripcion);
                }
            }
        }
    }

    public string Empresa
    {
        get => _empresa;
        private set => SetProperty(ref _empresa, value);
    }

    public string Descripcion
    {
        get => _descripcion;
        private set => SetProperty(ref _descripcion, value);
    }

    public string CategoriaTexto
    {
        get
        {
            if (Gasto?.Categorias == null) return "Desconocido";
            return Gasto.Categorias switch
            {
                Categoria.Entretenimiento => "Entretenimiento",
                Categoria.Comida => "Comida",
                Categoria.Transporte => "Transporte",
                Categoria.Ropa => "Ropa",
                Categoria.Educacion => "Educación",
                Categoria.Salud => "Salud",
                Categoria.ServiciosBasicos => "Servicios Básicos",
                Categoria.Otro => "Otro",
                _ => "Desconocido"
            };
        }
    }

    public string EstadoTexto
    {
        get
        {
            return Gasto?.Estados switch
            {
                Estado.Atrasado => "Atrasado",
                Estado.Finalizado => "Finalizado",
                Estado.Pendiente => "Pendiente",
                _ => "Desconocido"
            };
        }
    }

    public string ColorEstado => Gasto?.ColorEstado ?? "#FFFFFF";

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsNotLoading));
                ((Command)PagarGastoCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsNotLoading => !IsLoading;

    public double ValorPendiente => Math.Max(0, Gasto?.Valor ?? 0 - Gasto?.ValorPagado ?? 0);

    public bool EstaFinalizado => Gasto?.Estados == Estado.Finalizado;

    public ICommand RefreshCommand { get; }
    public ICommand PagarGastoCommand { get; }
    public ICommand EliminarGastoCommand { get; }
    public ICommand NavigateBackCommand { get; }

    public DetalleGastoViewModel(GastoService gastoService, int gastoId)
    {
        _gastoService = gastoService;
        _gastoId = gastoId;

        RefreshCommand = new Command(async () => await CargarGasto());
        PagarGastoCommand = new Command(
            async () => await PagarGastoAsync(),
            () => !EstaFinalizado && !IsLoading
        );
        EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
        NavigateBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        Task.Run(CargarGasto);
    }

    private void ExtraerEmpresaYDescripcion(string? descripcionCompleta)
    {
        if (string.IsNullOrEmpty(descripcionCompleta))
        {
            Empresa = "Sin empresa";
            Descripcion = "Sin descripción";
            return;
        }

        var partes = descripcionCompleta.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 2)
        {
            Empresa = partes[0].Trim();
            Descripcion = string.Join(" - ", partes.Skip(1)).Trim();
        }
        else
        {
            Empresa = "Otros";
            Descripcion = descripcionCompleta.Trim();
        }
    }

    private async Task CargarGasto()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            var gastos = await _gastoService.GetGastosAsync();
            var gasto = gastos.FirstOrDefault(g => g.IdGasto == _gastoId);

            if (gasto == null)
            {
                await MostrarError("No se encontró el gasto", true);
                return;
            }

            gasto.ValidarValor(); // Asegura que el estado y color estén actualizados
            Gasto = gasto;
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al cargar el gasto: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PagarGastoAsync()
    {
        if (Gasto == null || EstaFinalizado) return;

        var valorPagado = await Application.Current.MainPage.DisplayPromptAsync(
            "Pagar Gasto",
            $"Valor pendiente: {ValorPendiente.ToString("C", new CultureInfo("es-EC"))}\nIngrese el valor a pagar:",
            "Pagar",
            "Cancelar",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrEmpty(valorPagado)) return;

        if (!double.TryParse(valorPagado, NumberStyles.Any, CultureInfo.InvariantCulture, out double valor))
        {
            await MostrarError("El valor ingresado no es válido");
            return;
        }

        if (valor <= 0)
        {
            await MostrarError("El valor a pagar debe ser mayor a cero");
            return;
        }

        if (valor > ValorPendiente)
        {
            await MostrarError("El valor a pagar no puede ser mayor al valor pendiente");
            return;
        }

        var confirmar = await Application.Current.MainPage.DisplayAlert(
            "Confirmar Pago",
            $"¿Está seguro de pagar {valor.ToString("C", new CultureInfo("es-EC"))}?",
            "Sí",
            "No");

        if (!confirmar) return;

        try
        {
            IsLoading = true;
            var resultado = await _gastoService.PagarGastoAsync(Gasto.IdGasto, valor);

            if (resultado != null)
            {
                resultado.ValidarValor(); // Asegura que el estado y color estén actualizados
                Gasto = resultado;
                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Pago procesado correctamente",
                    "OK");
            }
            else
            {
                await MostrarError("No se pudo procesar el pago");
            }
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al procesar el pago: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EliminarGastoAsync()
    {
        if (Gasto == null) return;

        bool confirmar = await Application.Current.MainPage.DisplayAlert(
            "Confirmar Eliminación",
            "¿Está seguro de eliminar este gasto?",
            "Sí",
            "No"
        );

        if (!confirmar) return;

        try
        {
            IsLoading = true;
            bool eliminado = await _gastoService.DeleteGastoAsync(Gasto.IdGasto);

            if (eliminado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Gasto eliminado correctamente",
                    "OK");
                await Shell.Current.GoToAsync("///PaginaInicial");
            }
            else
            {
                await MostrarError("No se pudo eliminar el gasto");
            }
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al eliminar el gasto: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task MostrarError(string mensaje, bool navegar = false)
    {
        await Application.Current.MainPage.DisplayAlert("Error", mensaje, "OK");
        if (navegar)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}