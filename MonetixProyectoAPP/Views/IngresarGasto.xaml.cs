using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Views
{
    public partial class IngresarGasto : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        public IngresarGasto()
        {
            InitializeComponent();
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            var categorias = new List<string>
            {
                "Entretenimiento",
                "Comida",
                "Transporte",
                "Ropa",
                "Educacion",
                "Salud",
                "ServiciosBasicos"
            };

            CategoriaPicker.ItemsSource = categorias;
        }

        private async void OnGuardarGastoClicked(object sender, EventArgs e)
        {
            try
            {
                if (CategoriaPicker.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(DescripcionEntry.Text) ||
                    string.IsNullOrWhiteSpace(ValorEntry.Text) ||
                    !double.TryParse(ValorEntry.Text, out double valor))
                {
                    await DisplayAlert("Error", "Por favor, completa todos los campos correctamente.", "OK");
                    return;
                }

                var nuevoGasto = new Gasto
                {
                    FechaRegristo = FechaRegistroPicker.Date,
                    FechaFinal = FechaRegistroPicker.Date.AddDays(30),
                    Categorias = Enum.TryParse(CategoriaPicker.SelectedItem.ToString(), out Categoria categoriaSeleccionada)
                        ? categoriaSeleccionada
                        : Categoria.Otro,
                    Descripcion = DescripcionEntry.Text,
                    Valor = valor,
                    ValorPagado = 0,
                    Estados = Estado.Pendiente
                };

                var response = await _httpClient.PostAsJsonAsync("Gasto", nuevoGasto);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "El gasto se ha registrado correctamente.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar el gasto. Intente nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
