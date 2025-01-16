using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Views
{
    public partial class IngresarGasto : ContentPage
    {

        public IngresarGasto()
        {
            InitializeComponent();
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
