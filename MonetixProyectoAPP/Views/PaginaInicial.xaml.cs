using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views;

public partial class PaginaInicial : ContentPage
{
    private readonly PaginaInicialViewModel _viewModel;

    public PaginaInicial(GastoService gastoService)
    {
        InitializeComponent();
        _viewModel = new PaginaInicialViewModel(gastoService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100); // Give UI time to render
        await RefreshData();
    }

    private async Task RefreshData()
    {
        try
        {
            _viewModel.RefreshCommand.Execute(null);
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                "No se pudieron cargar los gastos: " + ex.Message,
                "OK");
        }
    }

    private async void OnGastoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Gasto gastoSeleccionado)
        {
            try
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "gastoId", gastoSeleccionado.IdGasto }
                };

                await Shell.Current.GoToAsync("DetalleGasto", navigationParameter);
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    "No se pudo abrir el detalle del gasto: " + ex.Message,
                    "OK");
            }
            finally
            {
                // Clear selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }

    private async void OnIngresarGastoClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("IngresarGasto");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                "No se pudo abrir la página de ingreso: " + ex.Message,
                "OK");
        }
    }

    private async void OnTiendasFavoritasClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("///TiendasFavoritasGuardadas");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                "No se pudo abrir la página de tiendas favoritas: " + ex.Message,
                "OK");
        }
    }
}