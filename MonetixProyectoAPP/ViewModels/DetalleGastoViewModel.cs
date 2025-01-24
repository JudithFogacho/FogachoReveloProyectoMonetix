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
        private readonly GastoService _gastoService;
        private readonly UsuarioService _usuarioService;
        private readonly ApiPublicaService _apiPublicaService = new ApiPublicaService();
        private readonly int _currentUserId;

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

        public DetalleGastoViewModel(Gasto gasto, UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _gastoService = new GastoService(_usuarioService);
            _currentUserId = _usuarioService.GetCurrentUserId();

            if (_currentUserId == 0)
            {
                Shell.Current.GoToAsync("///Login");
                return;
            }

            if (gasto.IdUsuario != _currentUserId)
            {
                Shell.Current.GoToAsync("///PaginaInicial");
                return;
            }

            Gasto = gasto;
            EliminarGastoCommand = new Command(async () => await EliminarGastoAsync());
            PagarGastoCommand = new Command(async () => await PagarGastoAsync());
            CargarLogo();
        }

        private async void CargarLogo()
        {
            if (Gasto == null || string.IsNullOrEmpty(Gasto.Descripcion))
            {
                LogoSource = "imagen_local_1.png";
                return;
            }

            try
            {
                var locales = await _apiPublicaService.GetLocalesPorCategoriaAsync(Gasto.Categorias.ToString());
                var empresa = locales?.FirstOrDefault(l => l.Nombre.Equals(Gasto.Descripcion, StringComparison.OrdinalIgnoreCase));

                if (empresa != null && !string.IsNullOrEmpty(empresa.Logo))
                {
                    LogoSource = empresa.Logo;
                }
                else
                {
                    var imagenesLocales = new List<string>
                   {
                       "imagen_local_1.png",
                       "imagen_local_2.png",
                       "imagen_local_3.png"
                   };

                    var random = new Random();
                    LogoSource = imagenesLocales[random.Next(imagenesLocales.Count)];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el logo: {ex.Message}");
                LogoSource = "imagen_local_1.png";
            }
        }

        private async Task EliminarGastoAsync()
        {
            if (_currentUserId == 0 || Gasto.IdUsuario != _currentUserId)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No tiene permisos para eliminar este gasto", "OK");
                return;
            }

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
            if (_currentUserId == 0 || Gasto.IdUsuario != _currentUserId)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No tiene permisos para pagar este gasto", "OK");
                return;
            }

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
                        try
                        {
                            var response = await _gastoService.PagarGastoAsync(Gasto.IdGasto, valor);

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
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error",
                                "No se pudo procesar el pago. Intente nuevamente.", "OK");
                            Console.WriteLine($"Error al procesar pago: {ex.Message}");
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
            input = input?.Replace("$", "").Replace(",", "").Trim();
            return double.TryParse(input, NumberStyles.Currency, CultureInfo.InvariantCulture, out valor);
        }
    }
}