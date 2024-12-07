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

        // Método para cargar las categorías en el Picker
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

        // Botón "Crear"
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
                    FechaFinal = FechaRegistroPicker.Date.AddDays(30), // Ejemplo: 30 días desde la fecha de registro
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

                // Aquí puedes guardar el nuevo gasto en tu base de datos o colección
                // Por ejemplo:
                // await App.Database.SaveGastoAsync(nuevoGasto);

                await DisplayAlert("Éxito", "El gasto se ha registrado correctamente.", "OK");

                // Regresar a la página anterior
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        // Botón "Cancelar"
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Regresa a la página anterior
        }
    }
}