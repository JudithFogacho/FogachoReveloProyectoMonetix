using Microsoft.Maui.Controls;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class TiendasFavoritas : ContentPage
    {
        public TiendasFavoritas(TiendasFavoritasViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel; // Asegúrate de asignar el ViewModel al BindingContext
        }
    }
}