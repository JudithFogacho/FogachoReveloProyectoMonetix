using Microsoft.Maui.Controls;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class Registro : ContentPage
    {
        public Registro(RegistroViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}