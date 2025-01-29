using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class IngresarGasto : ContentPage
    {
        private readonly IngresarGastoViewModel _viewModel;
        private bool _isNavigating;

        public IngresarGasto(GastoService gastoService)
        {
            InitializeComponent();
            _viewModel = new IngresarGastoViewModel(gastoService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatosIniciales();
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                if (_viewModel.Categorias.Count == 0)
                {
                    await _viewModel.LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                await MostrarError("No se pudieron cargar los datos iniciales", ex);
                await Regresar();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            CheckAndNavigateBack();
            return true;
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await CheckAndNavigateBack();
        }

        private async Task CheckAndNavigateBack()
        {
            if (_isNavigating) return;

            _isNavigating = true;
            try
            {
                if (await ConfirmarSalida())
                {
                    await Regresar();
                }
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async Task<bool> ConfirmarSalida()
        {
            if (HayCambiosPendientes())
            {
                return await DisplayAlert(
                    "Confirmar",
                    "¿Está seguro que desea salir? Se perderán los cambios no guardados.",
                    "Sí",
                    "No");
            }
            return true;
        }

        private bool HayCambiosPendientes()
        {
            var fechaDefault = DateTime.Now.AddDays(7).Date;
            return !string.IsNullOrWhiteSpace(_viewModel.Descripcion) ||
                   _viewModel.Valor > 0 ||
                   _viewModel.CategoriaSeleccionada != null ||
                   _viewModel.FechaFinal.Date != fechaDefault;
        }

        private async Task Regresar()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task MostrarError(string mensaje, Exception ex)
        {
            await DisplayAlert(
                "Error",
                $"{mensaje}: {ex.Message}",
                "OK");
        }
    }
}