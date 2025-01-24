using MonetixProyectoAPP.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace MonetixProyectoAPP.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;
        private string _email;
        private string _password;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await GoToRegisterAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Debe ingresar email y contraseña", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                var (success, message, userId) = await _usuarioService.LoginAsync(Email, Password);

                if (success)
                {
                    await HandleSuccessfulLogin();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error de autenticación", message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task HandleSuccessfulLogin()
        {
            // Obtener datos directamente del servicio
            var nombreCompleto = _usuarioService.CurrentUserName;
            var email = _usuarioService.CurrentUserEmail;

            if (!string.IsNullOrEmpty(nombreCompleto))
            {
                Preferences.Set("user_name", nombreCompleto);
            }

            // Actualizar estado de la aplicación
            MessagingCenter.Send(this, "UserLoggedIn");

            // Navegar a página principal
            await Shell.Current.GoToAsync("///PaginaInicial");
        }

        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("//Registro");
        }

        public void Logout()
        {
            _usuarioService.Logout();
            ResetUserData();
            Shell.Current.GoToAsync("//Login");
        }

        private void ResetUserData()
        {
            Email = string.Empty;
            Password = string.Empty;
            Preferences.Remove("user_name");
            MessagingCenter.Send(this, "UserLoggedOut");
        }
    }
}