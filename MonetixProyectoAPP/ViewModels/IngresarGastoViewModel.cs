using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Linq;

namespace MonetixProyectoAPP.ViewModels
{
    public class IngresarGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();

        // Propiedades para enlazar con la vista
        public List<string> Categorias { get; set; }
        private List<string> _empresas = new List<string>();
        public List<string> Empresas
        {
            get => _empresas;
            set => SetProperty(ref _empresas, value);
        }

        private string _empresaSeleccionada;
        public string EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set
            {
                SetProperty(ref _empresaSeleccionada, value);
                MostrarCampoTexto = value == "Otros";
            }
        }

        private bool _mostrarCampoTexto;
        public bool MostrarCampoTexto
        {
            get => _mostrarCampoTexto;
            set => SetProperty(ref _mostrarCampoTexto, value);
        }

        private DateTime _fechaRegistro = DateTime.Now;
        public DateTime FechaRegistro
        {
            get => _fechaRegistro;
            set => SetProperty(ref _fechaRegistro, value);
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

        // Comando para guardar el gasto
        public ICommand GuardarGastoCommand { get; }

        public IngresarGastoViewModel()
        {
            CargarCategorias();
            GuardarGastoCommand = new Command(async () => await GuardarGastoAsync());
        }

        private void CargarCategorias()
        {
            Categorias = Enum.GetNames(typeof(Categoria)).ToList();
        }

        private async void CargarEmpresasPorCategoria(string categoria)
        {
            var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(categoria);
            if (locales.Any())
            {
                Empresas = locales.Select(l => l.Nombre).ToList();
                Empresas.Add("Otros");
            }
            else
            {
                Empresas = new List<string> { "Otros" };
            }
            MostrarCampoTexto = false; // Resetear el campo de texto
        }

        private async Task GuardarGastoAsync()
        {
            // Validar que los campos no estén vacíos
            if (string.IsNullOrEmpty(CategoriaSeleccionada) ||
                string.IsNullOrEmpty(EmpresaSeleccionada) ||
                Valor <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, completa todos los campos correctamente.", "OK");
                return;
            }

            // Crear el objeto Gasto
            var nuevoGasto = new Gasto
            {
                FechaRegristo = FechaRegistro,
                FechaFinal = FechaFinal,
                Categorias = Enum.TryParse(CategoriaSeleccionada, out Categoria categoria)
                    ? categoria
                    : Categoria.Otro,
                Descripcion = EmpresaSeleccionada == "Otros" ? Descripcion : EmpresaSeleccionada, // Usar la empresa seleccionada o el texto ingresado
                Valor = Valor,
                ValorPagado = 0,
                Estados = Estado.Pendiente // Inicializar el estado como Pendiente
            };

            // Validar y actualizar el estado del gasto
            nuevoGasto.ValidarValor();
            nuevoGasto.AsignarColorEstado();

            // Enviar el gasto al servidor
            await _gastoService.CreateGastoAsync(nuevoGasto);

            // Mostrar mensaje de éxito y navegar a la página principal
            await Application.Current.MainPage.DisplayAlert("Éxito", "El gasto se ha registrado correctamente.", "OK");
            await Shell.Current.GoToAsync("///PaginaInicial");
        }
    }
}