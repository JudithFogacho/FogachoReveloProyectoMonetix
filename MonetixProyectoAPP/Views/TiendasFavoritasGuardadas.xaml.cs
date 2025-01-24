using Microsoft.Maui.Controls;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class TiendasFavoritasGuardadas : ContentPage
    {
        public TiendasFavoritasGuardadas(TiendasFavoritasGuardadasViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnAñadirMasTiendasClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("TiendasFavoritas");
        }
    }
}