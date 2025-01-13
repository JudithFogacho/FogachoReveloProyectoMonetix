using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MonetixProyectoAPP.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MonetixProyectoAPP.Views;

namespace MonetixProyectoAPP.ViewModels
{
    public class LoginViewModel: INotifyPropertyChanged
    {
        private readonly UsuarioService _service = new UsuarioService();

        private string _email;
        private string _password;

        public string Email {
            get => _email; 
            set { 
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        //Creación de metodos para que nos permita enlazar acciones en la interfaz de usuario
        public ICommand loginCommand { get;}

        public LoginViewModel() {
            loginCommand = new Command(async () => await LoginAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private async Task LoginAsync() {

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ) {
                await Application.Current.MainPage.DisplayAlert("ERROR", "Por favor ingrese email y contraseña", "OK");
                return;
            }

            try {
                var token = await _service.LoginAsync(Email, Password);
                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("auth_token", token);
                    await Application.Current.MainPage.Navigation.PushAsync(new PaginaInicial());
                }
                else { 
                    await Application.Current.MainPage.DisplayAlert("ERROR", "Credenciales Incorrectas", "OK");
                }
            } catch (Exception ex) {

                await Application.Current.MainPage.DisplayAlert("ERROR", $"Ocurrio un error: {ex.Message}", "OK");
            }
        }

    }
}
