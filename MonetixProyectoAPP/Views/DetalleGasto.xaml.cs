using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    [QueryProperty(nameof(GastoId), "gastoId")]
    public partial class DetalleGasto : ContentPage
    {
        private int _gastoId;
        private readonly GastoService _gastoService;
        private DetalleGastoViewModel _viewModel;

        public int GastoId
        {
            get => _gastoId;
            set
            {
                if (_gastoId != value)
                {
                    _gastoId = value;
                    LoadGasto(value);
                }
            }
        }

        public DetalleGasto(GastoService gastoService)
        {
            InitializeComponent();
            _gastoService = gastoService;
        }

        private void LoadGasto(int gastoId)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Create the ViewModel with the gastoId
                    _viewModel = new DetalleGastoViewModel(_gastoService, gastoId);
                    BindingContext = _viewModel;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await MostrarError("No se pudo cargar el detalle del gasto", ex);
                });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.RefreshCommand?.Execute(null);
        }

        protected override bool OnBackButtonPressed()
        {
            RegresarAsync();
            return true;
        }

        private async void RegresarAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task MostrarError(string mensaje, Exception ex)
        {
            await DisplayAlert("Error", $"{mensaje}: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}