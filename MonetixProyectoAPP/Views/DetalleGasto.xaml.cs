using Microsoft.Maui.Controls;
using MonetixProyectoAPP.Services;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    [QueryProperty(nameof(GastoId), "gastoId")]
    public partial class DetalleGasto : ContentPage
    {
        private int gastoId;
        public int GastoId
        {
            get => gastoId;
            set
            {
                gastoId = value;
                LoadGasto(value);
            }
        }

        private readonly GastoService _gastoService;

        public DetalleGasto(GastoService gastoService)
        {
            InitializeComponent();
            _gastoService = gastoService;
        }

        private void LoadGasto(int gastoId)
        {
            BindingContext = new DetalleGastoViewModel(_gastoService, gastoId);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}