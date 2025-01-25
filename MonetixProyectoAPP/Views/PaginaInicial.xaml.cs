using Microsoft.Maui.Controls;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshCommand.Execute(null);
    }

    private async void OnGastoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is GastoResponse gastoSeleccionado)
        {
            await Shell.Current.GoToAsync($"DetalleGasto?gastoId={gastoSeleccionado.IdGasto}");
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnIngresarGastoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("IngresarGasto");
    }

    private async void OnTiendasFavoritasClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///TiendasFavoritasGuardadas");
    }
}

public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string estado)
        {
            System.Diagnostics.Debug.WriteLine($"Estado recibido: {estado}");
            return estado.ToLower() switch
            {
                "atrasado" => Color.FromArgb("#E57373"),
                "pendiente" => Color.FromArgb("#FFD54F"),
                "finalizado" => Color.FromArgb("#81C784"),
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}