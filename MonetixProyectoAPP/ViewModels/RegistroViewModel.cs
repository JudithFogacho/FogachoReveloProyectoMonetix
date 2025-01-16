using MonetixProyectoAPP.Models;
using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonetixProyectoAPP.ViewModels
{
    public class RegistroViewModel : BaseViewModel
    {
        private readonly UsuarioService _service = new UsuarioService();

        private string _nombre;
        private string _apellido;
        private string _email;
        private string _password;
        public string Nombre {  get => _nombre; set=> SetProperty(ref _nombre, value); }
        public string Apellido { get => _apellido; set => SetProperty(ref _apellido, value); }

        public string Email { get => _email; set => SetProperty(ref _email, value); }

        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public ICommand RegisterCommand { get; }

        public ICommand GoToLoginCommand { get; }

        public RegistroViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await GoToLoginAsync());
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Apellido) ||
                string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password)) { 
                
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor complete todos los campos", "OK");
                return;
            }
            await ExecuteAsync(async () => {
                var nuevoUsuario = new Usuario { 
                    Nombre = Nombre,
                    Apellido = Apellido,
                    Email = Email,
                    Password = Password
                };

                await _service.CreateUsuarioAsync(nuevoUsuario);

                await Application.Current.MainPage.DisplayAlert("Éxito", "Registro completado con éxito", "OK");

                await Shell.Current.GoToAsync("///Login");
            });
        }

        private async Task GoToLoginAsync() {
            await Shell.Current.GoToAsync("///Login");
        }
    }
}
