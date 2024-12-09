using System.Collections.ObjectModel;
using MonetixProyectoAPP.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace MonetixProyectoAPP.Views;

public partial class PaginaInicial : ContentPage
{
    private readonly HttpClient _httpClient;
    public ObservableCollection<Gasto> Gastos { get; set; } = new();
    public ObservableCollection<Gasto> GastosFiltrados { get; set; } = new();
    public PaginaInicial()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/Gasto") 
        };
        BindingContext = this;
    }

    private async void OnIngresarGastoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IngresarGasto());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarGastos();
    }

    // Propiedades para los totales
    public decimal SubtotalGastos => (decimal)GastosFiltrados.Sum(g => g.Valor);
    public decimal SubtotalValorPagado => (decimal)GastosFiltrados.Sum(g => g.ValorPagado);
    public decimal TotalGastos => SubtotalGastos - SubtotalValorPagado;

    private async Task CargarGastos()
    {
        try
        {
            var gastos = await _httpClient.GetFromJsonAsync<List<Gasto>>("");
            if (gastos != null)
            {
                Gastos.Clear();
                foreach (var gasto in gastos)
                {
                    gasto.AsignarColorEstado(); // Método para asignar color al estado
                    Gastos.Add(gasto);
                }
                // Inicializamos GastosFiltrados con la lista completa al cargar
                GastosFiltrados = new ObservableCollection<Gasto>(Gastos);
                // Notificar que los totales han cambiado
                OnPropertyChanged(nameof(SubtotalGastos));
                OnPropertyChanged(nameof(SubtotalValorPagado));
                OnPropertyChanged(nameof(TotalGastos));
            }
        }
        catch (Exception ex)
        {
            // Maneja errores de conexión o datos aquí
            await DisplayAlert("Error", $"No se pudieron cargar los gastos: {ex.Message}", "OK");
        }
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var textoBusqueda = e.NewTextValue?.ToLower();
        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            // Si no hay texto, mostrar todos los gastos
            GastosFiltrados = new ObservableCollection<Gasto>(Gastos);
        }
        else
        {
            // Filtrar los gastos por la categoría que coincida con el texto ingresado
            GastosFiltrados = new ObservableCollection<Gasto>(
                Gastos.Where(g => g.Categorias.ToString().ToLower().Contains(textoBusqueda))
            );
        }

        // Actualizar la vista con los resultados filtrados
        GastosCollectionView.ItemsSource = GastosFiltrados;

        // Actualizar totales después del filtrado
        OnPropertyChanged(nameof(SubtotalGastos));
        OnPropertyChanged(nameof(SubtotalValorPagado));
        OnPropertyChanged(nameof(TotalGastos));
    }
    private async void OnGastoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
            if (e.CurrentSelection.FirstOrDefault() is Gasto gastoSeleccionado)
            {
                await Navigation.PushAsync(new DetalleGasto(gastoSeleccionado));
            }

        ((CollectionView)sender).SelectedItem = null;
        
    }
}
