using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System.Globalization;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace MonetixProyectoAPP.ViewModels
{
    public class DetalleGastoViewModel : BaseViewModel
    {
        private readonly GastoService _gastoService = new GastoService();
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        private Gasto _gasto;
        public Gasto Gasto
        {
            get => _gasto;
            set
            {
                SetProperty(ref _gasto, value);
                OnPropertyChanged(nameof(ValorPendiente));
                OnPropertyChanged(nameof(Estado));
                CargarLogo();
            }
        }

        private string _logoSource;
        public string LogoSource
        {
            get => _logoSource;
            set => SetProperty(ref _logoSource, value);
        }

        public double ValorPendiente => Gasto?.Valor.GetValueOrDefault() - Gasto?.ValorPagado ?? 0;

        public string Estado
        {
            get
            {
                if (ValorPendiente == 0)
                    return "Pagado";
                else if (Gasto.FechaFinal < DateTime.Now)
                    return "Atrasado";
                else
                    return "Pendiente";
            }
        }

        public ICommand EliminarGastoCommand { get; }
        public ICommand PagarGastoCommand { get; }

        public DetalleGastoViewModel(Gasto gasto)
        {
            Gasto = gasto;
            EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
            PagarGastoCommand = new Command(async () => await PagarGastoAsync());
            CargarLogo();
        }

        private async void CargarLogo()
        {
            if (Gasto == null || string.IsNullOrEmpty(Gasto.Descripcion))
            {
                LogoSource = "imagen_local_1.png"; // Imagen por defecto
                return;
            }

            try
            {
                // Obtener todas las empresas desde la API
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(Gasto.Categorias.ToString());

                // Buscar la empresa correspondiente en la lista de locales
                var empresa = locales?.FirstOrDefault(l => l.Nombre.Equals(Gasto.Descripcion, StringComparison.OrdinalIgnoreCase));

                if (empresa != null && !string.IsNullOrEmpty(empresa.Logo))
                {
                    // Si se encuentra la empresa, usar su logo
                    LogoSource = empresa.Logo;
                }
                else
                {
                    // Si no se encuentra, usar una de las imágenes locales
                    var imagenesLocales = new List<string>
                    {
                        "imagen_local_1.png",
                        "imagen_local_2.png",
                        "imagen_local_3.png"
                    };

                    // Seleccionar una imagen local al azar
                    var random = new Random();
                    LogoSource = imagenesLocales[random.Next(imagenesLocales.Count)];
                }
            }
            catch (Exception ex)
            {
                // Manejar errores de conexión o datos aquí
                Console.WriteLine($"Error al cargar el logo: {ex.Message}");
                LogoSource = "imagen_local_1.png"; // Imagen por defecto en caso de error
            }
        }

        private async Task EliminarGastoAsync()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Eliminación",
                "¿Está seguro de eliminar este gasto?",
                "Sí",
                "No"
            );

            if (confirmar)
            {
                await ExecuteAsync(async () =>
                {
                    await _gastoService.DeleteGastoAsync(Gasto.IdGasto);
                    await Shell.Current.GoToAsync("///PaginaInicial");
                });
            }
        }

        private async Task PagarGastoAsync()
        {
            var valorPagado = await Application.Current.MainPage.DisplayPromptAsync(
                "Pagar Gasto",
                "Ingrese el valor a pagar",
                "Pagar",
                "Cancelar",
                keyboard: Keyboard.Numeric
            );

            if (TryParseValor(valorPagado, out double valor))
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar Pago",
                    $"¿Está seguro de pagar {valor:C2}?",
                    "Sí",
                    "No"
                );

                if (confirmar)
                {
                    await ExecuteAsync(async () =>
                    {
                        var pagoData = new
                        {
                            ValorPagado = valor
                        };

                        var url = $"Gasto/Pagar/{Gasto.IdGasto}";
                        var content = new StringContent(JsonConvert.SerializeObject(pagoData), Encoding.UTF8, "application/json");
                        var response = await _httpClient.PutAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            await Application.Current.MainPage.DisplayAlert("Éxito", "El pago ha sido procesado correctamente", "OK");
                            await Shell.Current.GoToAsync("///PaginaInicial");
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
                        }
                    });
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El valor ingresado no es válido", "OK");
            }
        }

        private bool TryParseValor(string input, out double valor)
        {
            // Eliminar el símbolo de moneda y espacios
            input = input?.Replace("$", "").Replace(",", "").Trim();

            // Intentar convertir el valor a double
            return double.TryParse(input, NumberStyles.Currency, CultureInfo.InvariantCulture, out valor);
        }
    }
}