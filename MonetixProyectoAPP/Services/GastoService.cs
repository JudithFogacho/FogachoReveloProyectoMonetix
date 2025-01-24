using MonetixProyectoAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace MonetixProyectoAPP.Services
{
    public class GastoService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        private readonly UsuarioService _usuarioService;

        public GastoService(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public async Task<List<Gasto>> GetGastosAsync()
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0) return new List<Gasto>();

                var gastos = await _httpClient.GetFromJsonAsync<List<Gasto>>($"Gasto/usuario/{userId}");
                return gastos ?? new List<Gasto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los gastos: {ex.Message}");
                return new List<Gasto>();
            }
        }

        public async Task CreateGastoAsync(Gasto nuevoGasto)
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0) return;

                nuevoGasto.IdUsuario = userId;
                var response = await _httpClient.PostAsJsonAsync("Gasto", nuevoGasto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al crear el gasto: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear el gasto: {ex.Message}");
            }
        }

        public async Task DeleteGastoAsync(int idGasto)
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0) return;

                var response = await _httpClient.DeleteAsync($"Gasto/{idGasto}/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al eliminar el gasto: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el gasto: {ex.Message}");
            }
        }

        public async Task<HttpResponseMessage> PagarGastoAsync(int idGasto, double valorPago)
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);

                var pagoData = new { ValorPagado = valorPago };
                var content = new StringContent(
                    JsonSerializer.Serialize(pagoData),
                    Encoding.UTF8,
                    "application/json"
                );

                return await _httpClient.PutAsync($"Gasto/Pagar/{idGasto}/{userId}", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al realizar el pago: {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }

        public async Task<Gasto> GetGastoByIdAsync(int idGasto)
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0) return null;

                return await _httpClient.GetFromJsonAsync<Gasto>($"Gasto/{idGasto}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el gasto: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateGastoAsync(Gasto gasto)
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                if (userId == 0) return false;

                gasto.IdUsuario = userId;
                var response = await _httpClient.PutAsJsonAsync($"Gasto/{gasto.IdGasto}", gasto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el gasto: {ex.Message}");
                return false;
            }
        }
    }
}