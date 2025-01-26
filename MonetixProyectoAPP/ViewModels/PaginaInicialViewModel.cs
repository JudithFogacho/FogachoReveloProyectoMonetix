using System.Collections.ObjectModel;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.ViewModels;

public class PaginaInicialViewModel : BaseViewModel
{
    private readonly GastoService _gastoService;
    private ObservableCollection<GastoResponse> _gastos = new();
    private ObservableCollection<GastoResponse> _gastosFiltrados = new();
    private string _textoBusqueda;
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<GastoResponse> GastosFiltrados
    {
        get => _gastosFiltrados;
        set
        {
            SetProperty(ref _gastosFiltrados, value);
            OnPropertyChanged(nameof(GastosAtrasados));
            OnPropertyChanged(nameof(GastosPendientes));
            OnPropertyChanged(nameof(GastosFinalizados));
            OnPropertyChanged(nameof(ResumenGastos));
            OnPropertyChanged(nameof(ResumenPagado));
            OnPropertyChanged(nameof(ResumenPendiente));
        }
    }

    public string TextoBusqueda
    {
        get => _textoBusqueda;
        set
        {
            if (SetProperty(ref _textoBusqueda, value))
                AplicarFiltros();
        }
    }

    public int GastosAtrasados => GastosFiltrados?.Count(g => g.Estado == "Atrasado") ?? 0;
    public int GastosPendientes => GastosFiltrados?.Count(g => g.Estado == "Pendiente") ?? 0;
    public int GastosFinalizados => GastosFiltrados?.Count(g => g.Estado == "Finalizado") ?? 0;

    public double ResumenGastos => GastosFiltrados?.Sum(g => g.Valor) ?? 0;
    public double ResumenPagado => GastosFiltrados?.Sum(g => g.ValorPagado) ?? 0;
    public double ResumenPendiente => ResumenGastos - ResumenPagado;

    public Command RefreshCommand { get; }

    public PaginaInicialViewModel(GastoService gastoService)
    {
        _gastoService = gastoService;
        RefreshCommand = new Command(async () => await CargarGastos());
        Task.Run(CargarGastos);
    }

    private async Task CargarGastos()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var gastos = await _gastoService.GetGastosAsync();
            GastosFiltrados = new ObservableCollection<GastoResponse>(gastos.OrderByDescending(g => g.FechaRegistro));
            _gastos = new ObservableCollection<GastoResponse>(gastos);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AplicarFiltros()
    {
        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            GastosFiltrados = new ObservableCollection<GastoResponse>(_gastos.OrderByDescending(g => g.FechaRegistro));
            return;
        }

        var busqueda = TextoBusqueda.Trim().ToLower();
        var filtrados = _gastos.Where(g =>
            g.Descripcion?.ToLower().Contains(busqueda) == true ||
            g.Categoria?.ToLower().Contains(busqueda) == true ||
            g.Valor.ToString().Contains(busqueda) ||
            g.Estado?.ToLower().Contains(busqueda) == true)
        .OrderByDescending(g => g.FechaRegistro);

        GastosFiltrados = new ObservableCollection<GastoResponse>(filtrados);
    }
}