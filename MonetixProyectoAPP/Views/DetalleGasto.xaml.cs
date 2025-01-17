using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    

    public partial class DetalleGasto : ContentPage
    {
        public DetalleGasto(Gasto gasto)
        {
            InitializeComponent();

            BindingContext = new DetalleGastoViewModel(gasto);

        }


        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
