using MonetixProyectoAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net.Http;

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
            return await _httpClient.GetFromJsonAsync<List<Gasto>>("Gasto")?? new List<Gasto> ();

        }

        public async Task CreateGastoAsync (Gasto nuevoGasto)
        {
            await _httpClient.PostAsJsonAsync("Gasto", nuevoGasto);
        }

        public async Task DeleteGastoAsync(int idGasto)
        {
            await _httpClient.DeleteAsync($"Gasto/{idGasto}");
        }

    }
}
