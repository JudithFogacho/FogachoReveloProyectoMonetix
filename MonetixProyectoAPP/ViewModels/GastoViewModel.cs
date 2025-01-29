using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Collections.ObjectModel;

namespace MonetixProyectoAPP.ViewModels
{
    public class GastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService;
        private readonly ApiPublicaService _apiPublicaService = new();

        private ObservableCollection<Gasto> _gastos = new();
        private ResumenGastos _resumen;

        public ObservableCollection<Gasto> Gastos
        {
            get => _gastos;
            set => SetProperty(ref _gastos, value);
        }

        public ResumenGastos Resumen
        {
            get => _resumen;
            set => SetProperty(ref _resumen, value);
        }

        private ObservableCollection<string> _categorias = new();
        public ObservableCollection<string> Categorias
        {
            get => _categorias;
            set => SetProperty(ref _categorias, value);
        }

        // Filtro de estado actual
        private string _estadoFiltro;
        public string EstadoFiltro
        {
            get => _estadoFiltro;
            set
            {
                if (SetProperty(ref _estadoFiltro, value))
                {
                    LoadGastosAsync(value).ConfigureAwait(false);
                }
            }
        }

        public GastoViewModel(GastoService gastoService)
        {
            _gastoService = gastoService;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadGastosAsync();
            await LoadCategoriasAsync();
        }

        private async Task LoadGastosAsync(string? preference = null)
        {
            await ExecuteAsync(async () =>
            {
                var gastos = await _gastoService.GetGastosAsync(preference);
                Gastos = new ObservableCollection<Gasto>(gastos);
                Resumen = await _gastoService.GetResumenGastosAsync();
            });
        }

        private async Task LoadCategoriasAsync()
        {
            await ExecuteAsync(async () =>
            {
                try
                {
                    var tiendas = await _apiPublicaService.GetTiendasAsync();
                    var categoriasList = tiendas.Select(t => t.Categoria)
                                              .Distinct()
                                              .OrderBy(c => c)
                                              .ToList();
                    Categorias = new ObservableCollection<string>(categoriasList);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando categorías: {ex.Message}");
                    // Cargar categorías por defecto del enum
                    var defaultCategorias = Enum.GetNames(typeof(Categoria))
                                              .OrderBy(c => c)
                                              .ToList();
                    Categorias = new ObservableCollection<string>(defaultCategorias);
                }
            });
        }

        public async Task<List<string>> GetEmpresasPorCategoriaAsync(string categoria)
        {
            try
            {
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(categoria);
                var empresas = locales.Select(l => l.Nombre)
                                    .OrderBy(n => n)
                                    .ToList();
                empresas.Add("Otros");
                return empresas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando empresas: {ex.Message}");
                return new List<string> { "Otros" };
            }
        }

        public async Task CreateGastoAsync(
            DateTime fechaFinal,
            string categoriaStr,
            string empresa,
            string descripcion,
            double valor)
        {
            await ExecuteAsync(async () =>
            {
                if (!Enum.TryParse<Categoria>(categoriaStr, true, out Categoria categoria))
                {
                    throw new Exception("Categoría inválida");
                }

                var result = await _gastoService.CreateGastoAsync(
                    fechaFinal,
                    categoria,
                    empresa,
                    descripcion,
                    valor);

                if (result != null)
                {
                    await LoadGastosAsync(EstadoFiltro);
                }
                else
                {
                    throw new Exception("No se pudo crear el gasto");
                }
            });
        }

        public async Task DeleteGastoAsync(int idGasto)
        {
            await ExecuteAsync(async () =>
            {
                var success = await _gastoService.DeleteGastoAsync(idGasto);
                if (success)
                {
                    await LoadGastosAsync(EstadoFiltro);
                }
                else
                {
                    throw new Exception("No se pudo eliminar el gasto");
                }
            });
        }

        public async Task PagarGastoAsync(int idGasto, double valorPago)
        {
            await ExecuteAsync(async () =>
            {
                var result = await _gastoService.PagarGastoAsync(idGasto, valorPago);
                if (result != null)
                {
                    // Actualizar el gasto específico en la colección
                    var gastoIndex = Gastos.ToList().FindIndex(g => g.IdGasto == idGasto);
                    if (gastoIndex >= 0)
                    {
                        Gastos[gastoIndex] = result;
                    }

                    // Actualizar el resumen
                    Resumen = await _gastoService.GetResumenGastosAsync();
                }
                else
                {
                    throw new Exception("No se pudo procesar el pago");
                }
            });
        }

        public async Task UpdateGastoAsync(
            int idGasto,
            DateTime? fechaFinal = null,
            string? categoriaStr = null,
            string? descripcion = null,
            double? valor = null)
        {
            await ExecuteAsync(async () =>
            {
                Categoria? categoria = null;
                if (categoriaStr != null)
                {
                    if (!Enum.TryParse<Categoria>(categoriaStr, true, out Categoria parsedCategoria))
                    {
                        throw new Exception("Categoría inválida");
                    }
                    categoria = parsedCategoria;
                }

                var result = await _gastoService.UpdateGastoAsync(
                    idGasto,
                    fechaFinal,
                    categoria,
                    descripcion,
                    valor);

                if (result != null)
                {
                    await LoadGastosAsync(EstadoFiltro);
                }
                else
                {
                    throw new Exception("No se pudo actualizar el gasto");
                }
            });
        }

        // Métodos auxiliares para obtener gastos filtrados
        public IEnumerable<Gasto> GetGastosPendientes() =>
            Gastos.Where(g => g.Estados == Estado.Pendiente);

        public IEnumerable<Gasto> GetGastosAtrasados() =>
            Gastos.Where(g => g.Estados == Estado.Atrasado);

        public IEnumerable<Gasto> GetGastosFinalizados() =>
            Gastos.Where(g => g.Estados == Estado.Finalizado);
    }
}