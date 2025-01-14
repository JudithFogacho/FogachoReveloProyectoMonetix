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

        public async Task<string> LoginAsync(string email, string password) {
            var credenciales = new { Email = email, Password = password };

            var respuesta = await _httpClient.PostAsJsonAsync("Usuario/login", credenciales);

            if (respuesta.IsSuccessStatusCode) {
                var tokenResponse = await respuesta.Content.ReadFromJsonAsync<TokenResponse>();
                return tokenResponse?.Token;
            }
            return null;
        }

        public HttpClient GetAutheticatedHttpClient() {
            var token = Preferences.Get("auth_token", string.Empty);
            if (token == null) {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return _httpClient;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Usuario>>("Usuario") ?? new List<Usuario>();

        }

        public class TokenResponse { 
            public string Token { get; set; }
        }
        public async Task CreateUsuarioAsync(Usuario nuevoUsuario)
        {
            await _httpClient.PostAsJsonAsync("Usuario", nuevoUsuario);
        }
        public async Task DeleteUsuarioAsync(int idUsuario)
        {
            await _httpClient.DeleteAsync($"Usuario/{idUsuario}");
        }


    }
}
