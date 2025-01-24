using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        // Propiedad para almacenar el ID del usuario actual
        public int CurrentUserId { get; private set; }

        public async Task<(bool success, int userId)> LoginAsync(string email, string password)
        {
            var credenciales = new { Email = email, Password = password };
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync("Usuario/login", credenciales);
                if (respuesta.IsSuccessStatusCode)
                {
                    var usuario = await respuesta.Content.ReadFromJsonAsync<Usuario>();
                    if (usuario != null && usuario.Email == email && usuario.Password == password)
                    {
                        var token = "simulated-token";
                        // Guardar el token y el ID del usuario
                        Preferences.Set("auth_token", token);
                        Preferences.Set("user_id", usuario.IdUsuario);
                        CurrentUserId = usuario.IdUsuario;
                        return (true, usuario.IdUsuario);
                    }
                }
                return (false, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en LoginAsync: {ex.Message}");
                return (false, 0);
            }
        }

        public HttpClient GetAuthenticatedHttpClient()
        {
            var token = Preferences.Get("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return _httpClient;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                var httpClient = GetAuthenticatedHttpClient();
                return await httpClient.GetFromJsonAsync<List<Usuario>>("Usuario") ?? new List<Usuario>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUsuariosAsync: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public async Task<Usuario> GetUsuarioByIdAsync(int userId)
        {
            try
            {
                var httpClient = GetAuthenticatedHttpClient();
                return await httpClient.GetFromJsonAsync<Usuario>($"Usuario/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUsuarioByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateUsuarioAsync(Usuario nuevoUsuario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Usuario", nuevoUsuario);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CreateUsuarioAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int idUsuario)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Usuario/{idUsuario}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DeleteUsuarioAsync: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            // Limpiar las preferencias y el ID del usuario actual
            Preferences.Remove("auth_token");
            Preferences.Remove("user_id");
            CurrentUserId = 0;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public int GetCurrentUserId()
        {
            return Preferences.Get("user_id", 0);
        }
    }
}