using System.Collections.ObjectModel;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Globalization;

namespace MonetixProyectoAPP.ViewModels;

public class PaginaInicialViewModel : BaseViewModel
{
    private readonly GastoService _gastoService;
    private ObservableCollection<Gasto> _gastos = new();
    private ObservableCollection<Gasto> _gastosFiltrados = new();
    private ObservableCollection<CategoriaViewModel> _categorias = new();
    private ResumenGastos _resumen;
    private string _textoBusqueda;
    private CategoriaViewModel _categoriaSeleccionada;
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<Gasto> GastosFiltrados
    {
        get => _gastosFiltrados;
        set => SetProperty(ref _gastosFiltrados, value);
    }

    public ObservableCollection<CategoriaViewModel> Categorias
    {
        get => _categorias;
        set => SetProperty(ref _categorias, value);
    }

    public CategoriaViewModel CategoriaSeleccionada
    {
        get => _categoriaSeleccionada;
        set
        {
            if (SetProperty(ref _categoriaSeleccionada, value))
            {
                AplicarFiltros();
                OnPropertyChanged(nameof(MostrarTodas));
            }
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

    public ResumenGastos Resumen
    {
        get => _resumen;
        set
        {
            if (SetProperty(ref _resumen, value))
            {
                OnPropertyChanged(nameof(TotalGastos));
                OnPropertyChanged(nameof(GastosAtrasados));
                OnPropertyChanged(nameof(GastosPendientes));
                OnPropertyChanged(nameof(GastosFinalizados));
                OnPropertyChanged(nameof(ValorTotal));
                OnPropertyChanged(nameof(ValorPagado));
                OnPropertyChanged(nameof(ValorPendiente));
            }
        }
    }

    // Propiedades del resumen
    public string TotalGastos => Resumen?.TotalGastos.ToString() ?? "0";
    public string GastosAtrasados => Resumen?.GastosAtrasados.ToString() ?? "0";
    public string GastosPendientes => Resumen?.GastosPendientes.ToString() ?? "0";
    public string GastosFinalizados => Resumen?.GastosFinalizados.ToString() ?? "0";
    public string ValorTotal => FormatearMoneda(Resumen?.ValorTotal ?? 0);
    public string ValorPagado => FormatearMoneda(Resumen?.ValorPagado ?? 0);
    public string ValorPendiente => FormatearMoneda(Resumen?.ValorPendiente ?? 0);
    public bool MostrarTodas => CategoriaSeleccionada?.Nombre == "Todas";

    public Command RefreshCommand { get; }
    public Command LimpiarFiltrosCommand { get; }
    public Command<string> FiltrarPorEstadoCommand { get; }

    public PaginaInicialViewModel(GastoService gastoService)
    {
        _gastoService = gastoService;

        RefreshCommand = new Command(async () => await CargarDatosCompletos());
        LimpiarFiltrosCommand = new Command(LimpiarFiltros);
        FiltrarPorEstadoCommand = new Command<string>(async (estado) => await FiltrarPorEstado(estado));

        Task.Run(CargarDatosCompletos);
    }

    private async Task CargarDatosCompletos()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            await CargarCategorias();
            await CargarGastos();
            await ActualizarResumen();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarCategorias()
    {
        try
        {
            var categoriasList = new List<CategoriaViewModel>
            {
                new CategoriaViewModel { Nombre = "Todas" }
            };

            foreach (Categoria categoria in Enum.GetValues(typeof(Categoria)))
            {
                categoriasList.Add(new CategoriaViewModel
                {
                    Value = categoria,
                    Nombre = GetCategoriaNombre(categoria)
                });
            }

            Categorias = new ObservableCollection<CategoriaViewModel>(
                categoriasList.OrderBy(c => c.Nombre != "Todas").ThenBy(c => c.Nombre)
            );

            if (CategoriaSeleccionada == null)
            {
                CategoriaSeleccionada = Categorias.First(); // "Todas"
            }
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al cargar categorías: {ex.Message}");
        }
    }

    private async Task CargarGastos()
    {
        try
        {
            var gastos = await _gastoService.GetGastosAsync();
            if (gastos != null && gastos.Any())
            {
                foreach (var gasto in gastos)
                {
                    gasto.ValidarValor(); // Asegura que el estado y color estén actualizados
                }
                _gastos = new ObservableCollection<Gasto>(gastos);
                AplicarFiltros();
            }
            else
            {
                _gastos = new ObservableCollection<Gasto>();
                GastosFiltrados = new ObservableCollection<Gasto>();
            }
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al cargar gastos: {ex.Message}");
            _gastos = new ObservableCollection<Gasto>();
            GastosFiltrados = new ObservableCollection<Gasto>();
        }
    }

    private async Task ActualizarResumen()
    {
        try
        {
            Resumen = await _gastoService.GetResumenGastosAsync();
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al cargar el resumen: {ex.Message}");
        }
    }

    private void LimpiarFiltros()
    {
        TextoBusqueda = string.Empty;
        CategoriaSeleccionada = Categorias.First(); // "Todas"
    }

    private async Task FiltrarPorEstado(string estado)
    {
        try
        {
            var gastos = await _gastoService.GetGastosAsync(estado.ToLower());
            if (gastos != null && gastos.Any())
            {
                foreach (var gasto in gastos)
                {
                    gasto.ValidarValor(); // Asegura que el estado y color estén actualizados
                }
                _gastos = new ObservableCollection<Gasto>(gastos);
                AplicarFiltros();
            }
        }
        catch (Exception ex)
        {
            await MostrarError($"Error al filtrar por estado: {ex.Message}");
        }
    }

    private void AplicarFiltros()
    {
        var gastosFiltrados = _gastos.AsEnumerable();

        // Filtrar por categoría
        if (!MostrarTodas && CategoriaSeleccionada?.Value != null)
        {
            gastosFiltrados = gastosFiltrados.Where(g => g.Categorias == CategoriaSeleccionada.Value);
        }

        // Filtrar por texto de búsqueda
        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            string busqueda = TextoBusqueda.Trim().ToLower();
            gastosFiltrados = gastosFiltrados.Where(g =>
                g.Descripcion?.ToLower().Contains(busqueda) == true);
        }

        // Asegurar que los estados y colores estén actualizados
        foreach (var gasto in gastosFiltrados)
        {
            gasto.ValidarValor();
        }

        GastosFiltrados = new ObservableCollection<Gasto>(
            gastosFiltrados.OrderByDescending(g => g.FechaRegristo)
        );
    }

    private string GetCategoriaNombre(Categoria categoria)
    {
        return categoria switch
        {
            Categoria.Entretenimiento => "Entretenimiento",
            Categoria.Comida => "Comida",
            Categoria.Transporte => "Transporte",
            Categoria.Ropa => "Ropa",
            Categoria.Educacion => "Educación",
            Categoria.Salud => "Salud",
            Categoria.ServiciosBasicos => "Servicios Básicos",
            Categoria.Otro => "Otro",
            _ => categoria.ToString()
        };
    }

    private string FormatearMoneda(double valor)
    {
        return valor.ToString("C", new CultureInfo("es-EC"));
    }

    private async Task MostrarError(string mensaje)
    {
        await Application.Current.MainPage.DisplayAlert("Error", mensaje, "OK");
    }
}

