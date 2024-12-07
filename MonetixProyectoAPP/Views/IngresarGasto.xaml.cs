using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Views
{
    public partial class IngresarGasto : ContentPage
    {
        public IngresarGasto()
        {
            InitializeComponent();
            CargarCategorias();
        }

        // M�todo para cargar las categor�as en el Picker
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

        // Bot�n "Crear"
        private async void OnGuardarGastoClicked(object sender, EventArgs e)
        {
            try
            {
                // Validaciones de entrada
                if (CategoriaPicker.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(DescripcionEntry.Text) ||
                    string.IsNullOrWhiteSpace(ValorEntry.Text) ||
                    !double.TryParse(ValorEntry.Text, out double valor))
                {
                    await DisplayAlert("Error", "Por favor, completa todos los campos correctamente.", "OK");
                    return;
                }

                // Crear el objeto Gasto
                var nuevoGasto = new Gasto
                {
                    FechaRegristo = FechaRegistroPicker.Date,
                    FechaFinal = FechaRegistroPicker.Date.AddDays(30), // Ejemplo: 30 d�as desde la fecha de registro
                    Categorias = Enum.TryParse(CategoriaPicker.SelectedItem.ToString(), out Categoria categoriaSeleccionada)
                        ? categoriaSeleccionada
                        : Categoria.Otro,
                    Descripcion = DescripcionEntry.Text,
                    Valor = valor,
                    ValorPagado = 0, // Por defecto, inicialmente no se ha pagado nada
                    Estados = Estado.Pendiente // Estado inicial como "Pendiente"
                };

                // Asignar el color del estado
                nuevoGasto.AsignarColorEstado();

                // Aqu� puedes guardar el nuevo gasto en tu base de datos o colecci�n
                // Por ejemplo:
                // await App.Database.SaveGastoAsync(nuevoGasto);

                await DisplayAlert("�xito", "El gasto se ha registrado correctamente.", "OK");

                // Regresar a la p�gina anterior
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurri� un error: {ex.Message}", "OK");
            }
        }

        // Bot�n "Cancelar"
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Regresa a la p�gina anterior
        }
    }
}