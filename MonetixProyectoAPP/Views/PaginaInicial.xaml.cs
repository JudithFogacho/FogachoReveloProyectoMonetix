using System.Collections.ObjectModel;
using MonetixProyectoAPP.Models;
using System.Net.Http.Json;

namespace MonetixProyectoAPP.Views;

public partial class PaginaInicial : ContentPage
{
    private readonly HttpClient _httpClient;
    public ObservableCollection<Gasto> Gastos { get; set; } = new();
    public PaginaInicial()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/Gasto") // Cambia por la URL de tu API
        };
        BindingContext = this;
    }

    private void OnIngresarGastoClicked(object sender, EventArgs e)
    {
        // Lógica para agregar un nuevo gasto
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarGastos();
    }

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
            }
        }
        catch (Exception ex)
        {
            // Maneja errores de conexión o datos aquí
            await DisplayAlert("Error", $"No se pudieron cargar los gastos: {ex.Message}", "OK");
        }
    }
}
