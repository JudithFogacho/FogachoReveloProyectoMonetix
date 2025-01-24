using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.ViewModels
{
    public class IngresarGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly ApiPublicaService _apiPublicaService = new();

        public List<string> Categorias { get; }
        private List<string> _empresas = new();

        public List<string> Empresas
        {
            get => _empresas;
            set => SetProperty(ref _empresas, value);
        }

        private DateTime _fechaFinal = DateTime.Now;
        public DateTime FechaFinal
        {
            get => _fechaFinal;
            set => SetProperty(ref _fechaFinal, value);
        }

        private string _categoriaSeleccionada;
        public string CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                SetProperty(ref _categoriaSeleccionada, value);
                CargarEmpresasPorCategoria(value);
            }
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private double _valor;
        public double Valor
        {
            get => _valor;
            set => SetProperty(ref _valor, value);
        }

        public ICommand GuardarGastoCommand { get; }

        public IngresarGastoViewModel(GastoService gastoService)
        {
            _gastoService = gastoService;
            Categorias = Enum.GetNames(typeof(Categoria)).ToList();
            GuardarGastoCommand = new Command(async () => await GuardarGastoAsync());
        }

        private async void CargarEmpresasPorCategoria(string categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return;

            try
            {
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(categoria);
                Empresas = locales?.Select(l => l.Nombre).ToList() ?? new List<string>();
                Empresas.Add("Otros");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando empresas: {ex.Message}");
                Empresas = new List<string> { "Otros" };
            }
        }

        private async Task GuardarGastoAsync()
        {
            if (!ValidarDatos())
            {
                await Shell.Current.DisplayAlert("Error",
                    "Por favor completa todos los campos correctamente", "OK");
                return;
            }

            await ExecuteAsync(async () =>
            {
                await _gastoService.CreateGastoAsync(
                    fechaFinal: FechaFinal,
                    categoria: CategoriaSeleccionada,
                    descripcion: Descripcion,
                    valor: Valor
                );

                await Shell.Current.DisplayAlert("Éxito",
                    "Gasto registrado correctamente", "OK");
                await Shell.Current.GoToAsync("///PaginaInicial");
                LimpiarCampos();
            });
        }

        private bool ValidarDatos()
        {
            return !string.IsNullOrWhiteSpace(CategoriaSeleccionada) &&
                   !string.IsNullOrWhiteSpace(Descripcion) &&
                   Valor > 0 &&
                   FechaFinal >= DateTime.Today;
        }

        private void LimpiarCampos()
        {
            FechaFinal = DateTime.Now;
            CategoriaSeleccionada = null;
            Descripcion = string.Empty;
            Valor = 0;
            Empresas = new List<string>();
        }
    }
}