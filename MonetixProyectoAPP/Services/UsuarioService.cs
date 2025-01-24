using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using MonetixProyectoAPP.Models;

namespace MonetixProyectoAPP.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/usuarios/")
        };

        // Propiedades para acceder a los datos del usuario
        public int CurrentUserId => Preferences.Get("user_id", 0);
        public string CurrentUserEmail => Preferences.Get("user_email", string.Empty);
        public string CurrentUserName => $"{Preferences.Get("user_nombre", "")} {Preferences.Get("user_apellido", "")}";

        public async Task<(bool success, string message, int userId)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", new
                {
                    Email = email,
                    Password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    return (false, error?.Message ?? "Error desconocido", 0);
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                // Almacenar datos del usuario
                Preferences.Set("user_id", loginResponse.IdUsuario);
                Preferences.Set("user_email", loginResponse.Email);
                Preferences.Set("user_nombre", loginResponse.Nombre);
                Preferences.Set("user_apellido", loginResponse.Apellido);

                return (true, "Login exitoso", loginResponse.IdUsuario);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string nombre, string apellido, string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("register", new
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = email,
                    Password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    return (false, error?.Message ?? "Error en el registro");
                }

                // Opcional: Auto-login después del registro
                if (response.IsSuccessStatusCode)
                {
                    var usuarioRegistrado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
                    if (usuarioRegistrado != null)
                    {
                        Preferences.Set("user_id", usuarioRegistrado.IdUsuario);
                        Preferences.Set("user_email", usuarioRegistrado.Email);
                        Preferences.Set("user_nombre", usuarioRegistrado.Nombre);
                        Preferences.Set("user_apellido", usuarioRegistrado.Apellido);
                    }
                }

                return (true, "Registro exitoso");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public UsuarioResponse GetUserProfile()
        {
            if (CurrentUserId == 0) return null;

            return new UsuarioResponse(
                CurrentUserId,
                Preferences.Get("user_nombre", string.Empty),
                Preferences.Get("user_apellido", string.Empty),
                CurrentUserEmail);
        }

        public void Logout()
        {
            // Limpiar todos los datos de usuario
            Preferences.Remove("user_id");
            Preferences.Remove("user_email");
            Preferences.Remove("user_nombre");
            Preferences.Remove("user_apellido");
        }

        // Método para actualizar datos locales
        public void UpdateLocalProfile(string nombre, string apellido)
        {
            Preferences.Set("user_nombre", nombre);
            Preferences.Set("user_apellido", apellido);
        }
    }

    // Modelos actualizados
    public record UsuarioResponse(
        int IdUsuario,
        string Nombre,
        string Apellido,
        string Email);

    public record LoginResponse(
        int IdUsuario,
        string Nombre,
        string Apellido,
        string Email);

    public record ErrorResponse(string Message);
}