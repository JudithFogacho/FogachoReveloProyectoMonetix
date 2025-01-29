using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Globalization;

namespace MonetixProyectoAPP.ViewModels
{
    public class IngresarGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly ApiPublicaService _apiPublicaService = new();
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsNotLoading));
                    GuardarGastoCommand.ChangeCanExecute();
                }
            }
        }

        private List<CategoriaViewModel> _categorias = new();
        public List<CategoriaViewModel> Categorias
        {
            get => _categorias;
            set => SetProperty(ref _categorias, value);
        }

        private List<string> _empresas = new();
        public List<string> Empresas
        {
            get => _empresas;
            set => SetProperty(ref _empresas, value);
        }

        private DateTime _fechaFinal = DateTime.Now.AddDays(7);
        public DateTime FechaFinal
        {
            get => _fechaFinal;
            set => SetProperty(ref _fechaFinal, value);
        }

        public DateTime FechaMinima => DateTime.Today;

        private CategoriaViewModel _categoriaSeleccionada;
        public CategoriaViewModel CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                if (SetProperty(ref _categoriaSeleccionada, value))
                {
                    CargarEmpresasPorCategoria(value?.Nombre);
                    GuardarGastoCommand.ChangeCanExecute();
                }
            }
        }

        private string _empresaSeleccionada = "Otros";
        public string EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set
            {
                if (SetProperty(ref _empresaSeleccionada, value))
                {
                    GuardarGastoCommand.ChangeCanExecute();
                }
            }
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (SetProperty(ref _descripcion, value))
                {
                    GuardarGastoCommand.ChangeCanExecute();
                }
            }
        }

        private double _valor;
        public double Valor
        {
            get => _valor;
            set
            {
                if (SetProperty(ref _valor, value))
                {
                    GuardarGastoCommand.ChangeCanExecute();
                }
            }
        }

        public Command GuardarGastoCommand { get; }
        public ICommand CargarDatosCommand { get; }

        public IngresarGastoViewModel(GastoService gastoService)
        {
            _gastoService = gastoService;
            GuardarGastoCommand = new Command(async () => await GuardarGastoAsync(), CanGuardarGasto);
            CargarDatosCommand = new Command(async () => await LoadDataAsync());

            MainThread.BeginInvokeOnMainThread(async () => await LoadDataAsync());
        }

        private bool CanGuardarGasto()
        {
            return CategoriaSeleccionada != null &&
                   !string.IsNullOrWhiteSpace(EmpresaSeleccionada) &&
                   !string.IsNullOrWhiteSpace(Descripcion) &&
                   Valor > 0 &&
                   FechaFinal >= DateTime.Today &&
                   !IsLoading;
        }

        private async void CargarEmpresasPorCategoria(string categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return;

            try
            {
                IsLoading = true;
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(categoria);
                Empresas = locales?.Select(l => l.Nombre)
                                 .OrderBy(n => n)
                                 .ToList() ?? new List<string>();

                if (!Empresas.Contains("Otros"))
                {
                    Empresas.Add("Otros");
                }

                if (!Empresas.Contains(EmpresaSeleccionada))
                {
                    EmpresaSeleccionada = "Otros";
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Error",
                        $"Error al cargar empresas: {ex.Message}", "OK");
                });
                Empresas = new List<string> { "Otros" };
                EmpresaSeleccionada = "Otros";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GuardarGastoAsync()
        {
            if (!ValidarDatos())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Error",
                        "Por favor complete todos los campos correctamente", "OK");
                });
                return;
            }

            try
            {
                IsLoading = true;
                string descripcionCompleta = FormatearDescripcion();

                var resultado = await _gastoService.CreateGastoAsync(
                    fechaFinal: FechaFinal,
                    categoria: CategoriaSeleccionada.Value,
                    empresa: EmpresaSeleccionada,
                    descripcion: Descripcion,
                    valor: Valor
                );

                if (resultado != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.DisplayAlert("Éxito",
                            "Gasto registrado correctamente", "OK");
                        await Shell.Current.GoToAsync("///PaginaInicial");
                    });
                    LimpiarCampos();
                }
                else
                {
                    throw new Exception("No se pudo registrar el gasto");
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Error",
                        $"Error al guardar el gasto: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string FormatearDescripcion()
        {
            return EmpresaSeleccionada == "Otros"
                ? Descripcion.Trim()
                : $"{EmpresaSeleccionada.Trim()} - {Descripcion.Trim()}";
        }

        private bool ValidarDatos()
        {
            return CategoriaSeleccionada != null &&
                   !string.IsNullOrWhiteSpace(EmpresaSeleccionada) &&
                   !string.IsNullOrWhiteSpace(Descripcion) &&
                   Valor > 0 &&
                   FechaFinal >= DateTime.Today;
        }

        private void LimpiarCampos()
        {
            FechaFinal = DateTime.Now.AddDays(7);
            CategoriaSeleccionada = null;
            EmpresaSeleccionada = "Otros";
            Descripcion = string.Empty;
            Valor = 0;
            Empresas = new List<string> { "Otros" };
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var categoriasList = new List<CategoriaViewModel>();

                foreach (Categoria categoria in Enum.GetValues(typeof(Categoria)))
                {
                    categoriasList.Add(new CategoriaViewModel
                    {
                        Value = categoria,
                        Nombre = GetCategoriaNombre(categoria)
                    });
                }

                Categorias = categoriasList.OrderBy(c => c.Nombre).ToList();
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Error",
                        $"Error al cargar categorías: {ex.Message}", "OK");
                });

                // Cargar categorías por defecto
                var defaultCategorias = new List<CategoriaViewModel>();
                foreach (Categoria categoria in Enum.GetValues(typeof(Categoria)))
                {
                    defaultCategorias.Add(new CategoriaViewModel
                    {
                        Value = categoria,
                        Nombre = GetCategoriaNombre(categoria)
                    });
                }
                Categorias = defaultCategorias.OrderBy(c => c.Nombre).ToList();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetCategoriaNombre(Categoria categoria)
        {
            return categoria switch
            {
                Categoria.Entretenimiento => "Entretenimiento",
                Categoria.Comida => "Comida",
                Categoria.Transporte => "Transporte",
                Categoria.Ropa => "Ropa",
                Categoria.Educacion => "Educación",
                Categoria.Salud => "Salud",
                Categoria.ServiciosBasicos => "Servicios Básicos",
                Categoria.Otro => "Otro",
                _ => categoria.ToString()
            };
        }
    }

    public class CategoriaViewModel
    {
        public Categoria Value { get; set; }
        public string Nombre { get; set; }
    }
}