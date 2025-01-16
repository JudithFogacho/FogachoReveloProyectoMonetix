using System.Net.Http.Json;
using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.ViewModels;
namespace MonetixProyectoAPP.Views;


public partial class Login : ContentPage
{

    public Login()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("Registro");
    }
}