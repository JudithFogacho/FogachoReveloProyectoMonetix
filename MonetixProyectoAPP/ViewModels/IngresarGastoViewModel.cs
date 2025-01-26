using System;
using System.Collections.Generic;
using System.Globalization;
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

        private DateTime _fechaInicio = DateTime.Now;
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set => SetProperty(ref _fechaInicio, value);
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

        private string _empresaSeleccionada;
        public string EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set => SetProperty(ref _empresaSeleccionada, value);
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
            Categorias = new List<string> {
           "Entretenimiento",
           "Comida",
           "Transporte",
           "Ropa",
           "Educacion",
           "Salud",
           "ServiciosBasicos",
           "Otro"
       };
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
                await Shell.Current.DisplayAlert("Error", "Por favor completa todos los campos correctamente", "OK");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var descripcionFinal = $"{EmpresaSeleccionada} - {Descripcion}";

                await _gastoService.CreateGastoAsync(
                    fechaFinal: FechaFinal,
                    categoria: CategoriaSeleccionada,
                    descripcion: descripcionFinal,
                    valor: Valor
                );

                await Shell.Current.DisplayAlert("Éxito", "Gasto registrado correctamente", "OK");
                await Shell.Current.GoToAsync("///PaginaInicial");
                LimpiarCampos();
            });
        }

        private bool ValidarDatos()
        {
            return !string.IsNullOrWhiteSpace(CategoriaSeleccionada) &&
                   !string.IsNullOrWhiteSpace(EmpresaSeleccionada) &&
                   !string.IsNullOrWhiteSpace(Descripcion) &&
                   Valor > 0 &&
                   FechaFinal >= FechaInicio;
        }

        private void LimpiarCampos()
        {
            FechaInicio = DateTime.Now;
            FechaFinal = DateTime.Now;
            CategoriaSeleccionada = null;
            EmpresaSeleccionada = null;
            Descripcion = string.Empty;
            Valor = 0;
            Empresas = new List<string>();
        }
    }

    public class EstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "0" => "Atrasado",
                "1" => "Pendiente",
                "2" => "Finalizado",
                _ => "Desconocido"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoriaConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> Categorias = new()
   {
       { "0", "Otro" },
       { "1", "Entretenimiento" },
       { "2", "Comida" },
       { "3", "Transporte" },
       { "4", "Ropa" },
       { "5", "Educacion" },
       { "6", "Salud" },
       { "7", "ServiciosBasicos" }
   };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Otro";
            return Categorias.TryGetValue(value.ToString(), out var categoria) ? categoria : "Otro";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EstadoColorConverter : IValueConverter
    {
        private static readonly Dictionary<string, Color> ColorEstados = new()
   {
       { "0", Color.Parse("#E57373") },
       { "1", Color.Parse("#FFD54F") },
       { "2", Color.Parse("#81C784") }
   };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Colors.Gray;
            return ColorEstados.TryGetValue(value.ToString(), out var color) ? color : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}