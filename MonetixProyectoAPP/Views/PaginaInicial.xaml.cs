using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;
using System.Globalization;

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

public class EstadoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return "Desconocido";

        return value.ToString() switch
        {
            "2" => "Finalizado",
            "1" => "Pendiente",
            "0" => "Atrasado",
            _ => "Desconocido"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoriaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return "Otro";

        return value.ToString() switch
        {
            "1" => "Entretenimiento",
            "2" => "Comida",
            "3" => "Transporte",
            "4" => "Ropa",
            "5" => "Educación",
            "6" => "Salud",
            "7" => "Servicios Básicos",
            _ => "Otro"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Colors.Gray;

        return value.ToString() switch
        {
            "2" => Color.Parse("#81C784"),  // Finalizado
            "1" => Color.Parse("#FFD54F"),  // Pendiente
            "0" => Color.Parse("#E57373"),  // Atrasado
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}