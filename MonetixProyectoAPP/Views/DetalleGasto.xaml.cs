namespace MonetixProyectoAPP.Views
{
    using MonetixProyectoAPP.Models;
    using System;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;
    using System.Text;
    using System.IO;
    using System.Globalization;
    using System.Net.Http.Json;
    using MonetixProyectoAPP.ViewModels;

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
