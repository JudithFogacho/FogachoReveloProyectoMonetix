using MonetixProyectoAPP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.Services
{
    public class ApiPublicaService
    {
        private readonly HttpClient _httpClient;

        public ApiPublicaService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<Tienda>> GetTiendasAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://67931d7f5eae7e5c4d8d994e.mockapi.io/Categorias");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Tienda>>(content);
            }
            return new List<Tienda>();
        }

        public async Task<List<Tienda>> GetLocalesPorCategoriaAsync(string categoria)
        {
            var tiendas = await GetTiendasAsync();
            return tiendas.Where(t => t.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}