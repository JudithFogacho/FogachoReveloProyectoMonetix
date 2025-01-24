using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class IngresarGasto : ContentPage
    {
        public IngresarGasto(GastoService gastoService)
        {
            InitializeComponent();
            BindingContext = new IngresarGastoViewModel(gastoService);
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}