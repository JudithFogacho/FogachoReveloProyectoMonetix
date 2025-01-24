using MonetixProyectoAPP.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace MonetixProyectoAPP.ViewModels
{
    public class RegistroViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;
        private string _nombre;
        private string _apellido;
        private string _email;
        private string _password;

        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        public string Apellido
        {
            get => _apellido;
            set => SetProperty(ref _apellido, value);
        }

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

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegistroViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await GoToLoginAsync());
        }

        private async Task RegisterAsync()
        {
            if (!ValidateInputs())
            {
                await Shell.Current.DisplayAlert("Error", "Todos los campos son requeridos", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                var (success, message) = await _usuarioService.RegisterAsync(
                    Nombre,
                    Apellido,
                    Email,
                    Password
                );

                if (success)
                {
                    await HandleSuccessfulRegistration();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error en registro", message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateInputs()
        {
            return !string.IsNullOrWhiteSpace(Nombre) &&
                   !string.IsNullOrWhiteSpace(Apellido) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password.Length >= 6;
        }

        private async Task HandleSuccessfulRegistration()
        {
            ClearSensitiveData();
            await Shell.Current.DisplayAlert("Éxito", "Registro completado correctamente", "OK");

            // Navegar directamente a la página principal
            await Shell.Current.GoToAsync("///PaginaInicial");
        }

        private void ClearSensitiveData()
        {
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}