using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class PaginaInicial : ContentPage
    {
        private readonly GastoService _gastoService;

        public PaginaInicial(GastoService gastoService)
        {
            InitializeComponent();
            _gastoService = gastoService;
            BindingContext = new PaginaInicialViewModel(_gastoService);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PaginaInicialViewModel viewModel)
            {
                viewModel.CargarGastos();
            }
        }

        private async void OnIngresarGastoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("IngresarGasto");
        }

        private async void OnTiendasFavoritasClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///TiendasFavoritasGuardadas");
        }

        private async void OnGastoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is GastoResponse gastoSeleccionado)
            {
                await Shell.Current.GoToAsync($"DetalleGasto?gastoId={gastoSeleccionado.IdGasto}");
            }
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    public class EstadoColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (string)value switch
            {
                "Atrasado" => Color.FromArgb("#E57373"),
                "Pendiente" => Color.FromArgb("#FFD54F"),
                "Finalizado" => Color.FromArgb("#81C784"),
                _ => Colors.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}