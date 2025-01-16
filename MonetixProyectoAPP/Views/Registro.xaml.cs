using System.Net.Http.Json;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.ViewModels;

namespace MonetixProyectoAPP.Views
{
    public partial class Registro : ContentPage
    {

        public Registro()
        {
            InitializeComponent();
            BindingContext = new RegistroViewModel();
        }
    }
}
