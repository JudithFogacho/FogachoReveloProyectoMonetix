using MonetixProyectoAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json;

namespace MonetixProyectoAPP.Services
{
    public class GastoService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7156/api/")
        };

        public async Task <List<Gasto>> GetGastosAsync()
        {
            try
            {
                var gastos = await _httpClient.GetFromJsonAsync<List<Gasto>>("Gasto");
                return gastos ?? new List<Gasto>();
            }
            catch (Exception ex) { 
                Console.WriteLine($"Error al obtener los gastos: {ex.Message}");
                return new List<Gasto>();
            }

        }

        public async Task CreateGastoAsync (Gasto nuevoGasto)
        {
            try {
                var response = await _httpClient.PostAsJsonAsync("Gasto", nuevoGasto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al crear el gasto: {errorMessage}");
                }
            }catch (Exception ex) { 
                Console.WriteLine($"Error al crear el gasto: {ex.Message}"); 
            }
        }

        public async Task DeleteGastoAsync(int idGasto)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Gasto/{idGasto}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al eliminar el gasto: {errorMessage}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error al eliminar el gasto: {ex.Message}");
            }
        }

        public async Task<HttpResponseMessage> PagarGastoAsync(object pagoData)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(pagoData), Encoding.UTF8, "application7json");
                return await _httpClient.PutAsync("Gasto/Pagar", content);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error al realizar el pago: {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
