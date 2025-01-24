using MonetixProyectoAPP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MonetixProyectoAPP.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MonetixProyectoAPP.Views;

namespace MonetixProyectoAPP.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UsuarioService _service = new UsuarioService();
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

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await GoToRegister());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Application.Current.MainPage.DisplayAlert("ERROR", "Por favor ingrese email y contraseña", "OK");
                return;
            }

            try
            {
                await ExecuteAsync(async () =>
                {
                    var (success, userId) = await _service.LoginAsync(Email, Password);
                    if (success && userId > 0)
                    {
                        // Guardar tanto el token como el ID del usuario
                        Preferences.Set("auth_token", "simulated-token");
                        Preferences.Set("user_id", userId);

                        // Notificar al GastoViewModel del cambio de usuario
                        MessagingCenter.Send(this, "UserLoggedIn", userId);

                        await Shell.Current.GoToAsync("///PaginaInicial");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("ERROR", "Credenciales Incorrectas", "OK");
                    }
                });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo conectar al servidor. Verifica tu conexión a internet.", "OK");
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Ocurrió un error inesperado. Inténtalo más tarde.", "OK");
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }
        }

        private async Task GoToRegister()
        {
            await Shell.Current.GoToAsync("///Registro");
        }

        public void Logout()
        {
            // Limpiar las preferencias
            Preferences.Remove("auth_token");
            Preferences.Remove("user_id");

            // Notificar a otros ViewModels del logout
            MessagingCenter.Send(this, "UserLoggedOut");

            // Limpiar campos
            Email = string.Empty;
            Password = string.Empty;

            // Redirigir al login
            Shell.Current.GoToAsync("///Login");
        }
    }
}