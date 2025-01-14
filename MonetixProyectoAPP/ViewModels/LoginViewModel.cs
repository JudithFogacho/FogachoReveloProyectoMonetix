﻿using MonetixProyectoAPP.Services;
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
    public class LoginViewModel: BaseViewModel
    {
        private readonly UsuarioService _service = new UsuarioService();

        private string _email;
        private string _password;

        public string Email {
            get => _email; 
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        //Creación de metodos para que nos permita enlazar acciones en la interfaz de usuario
        public ICommand loginCommand { get;}

        public LoginViewModel() {
            loginCommand = new Command(async () => await LoginAsync());
        }


        private async Task LoginAsync() {

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ) {
                await Application.Current.MainPage.DisplayAlert("ERROR", "Por favor ingrese email y contraseña", "OK");
                return;
            }

            try
            {

                await ExecuteAsync(async () =>
                {
                    var token = await _service.LoginAsync(Email, Password);
                    if (!string.IsNullOrEmpty(token))
                    {
                        Preferences.Set("auth_token", token);
                        await Shell.Current.GoToAsync("///PaginaInicial");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("ERROR", "Credenciales Incorrectas", "OK");
                    }
                });


            }
            catch (System.Net.Http.HttpRequestException ex){
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo conectar al servidor. Verifica tu conexion a internet.", "OK");
                Console.WriteLine($"Error de conexion: {ex.Message}");


            }catch (Exception ex)
            {

                await Application.Current.MainPage.DisplayAlert("Error", "Ocurrio un error inesperado. Intentalo más tarde.", "OK");
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }
        }

    }
}
