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
        private readonly GastoService _gastoService;
        private readonly UsuarioService _usuarioService;
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();
        private readonly int _currentUserId;

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

        public ICommand GuardarGastoCommand { get; }

        public IngresarGastoViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _gastoService = new GastoService(_usuarioService);
            _currentUserId = _usuarioService.GetCurrentUserId();

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
            MostrarCampoTexto = false;
        }

        private async Task GuardarGastoAsync()
        {
            if (_currentUserId == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe iniciar sesión para registrar gastos.", "OK");
                await Shell.Current.GoToAsync("///Login");
                return;
            }

            if (string.IsNullOrEmpty(CategoriaSeleccionada) ||
                string.IsNullOrEmpty(EmpresaSeleccionada) ||
                Valor <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, completa todos los campos correctamente.", "OK");
                return;
            }

            var nuevoGasto = new Gasto
            {
                IdUsuario = _currentUserId,
                FechaRegristo = FechaRegistro,
                FechaFinal = FechaFinal,
                Categorias = Enum.TryParse(CategoriaSeleccionada, out Categoria categoria)
                    ? categoria
                    : Categoria.Otro,
                Descripcion = EmpresaSeleccionada == "Otros" ? Descripcion : EmpresaSeleccionada,
                Valor = Valor,
                ValorPagado = 0,
                Estados = Estado.Pendiente
            };

            try
            {
                nuevoGasto.ValidarValor();
                nuevoGasto.AsignarColorEstado();

                await _gastoService.CreateGastoAsync(nuevoGasto);

                await Application.Current.MainPage.DisplayAlert("Éxito", "El gasto se ha registrado correctamente.", "OK");
                await Shell.Current.GoToAsync("///PaginaInicial");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"No se pudo registrar el gasto: {ex.Message}", "OK");
            }
        }
    }
}