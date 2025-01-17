using System.Collections.ObjectModel;
using MonetixProyectoAPP.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views;

public partial class PaginaInicial : ContentPage
{
    public PaginaInicial()
    {
        InitializeComponent();

        BindingContext = new PaginaInicialViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PaginaInicialViewModel viewModel) 
        {
            await viewModel.CargarGastos();
        }
    }

    private async void OnIngresarGastoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IngresarGasto());
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
