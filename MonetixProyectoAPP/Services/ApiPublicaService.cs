using MonetixProyectoAPP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.Services
{
    public class ApiPublicaService
    {
        private readonly HttpClient _httpClient;

        public ApiPublicaService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.jsondataai.com/api/0QWtBTt")
            };
        }

        public async Task<List<Local>> GetLocalesPorCategoriaAsync(string categoria)
        {
            try
            {
                // Obtener todos los locales desde la API
                var locales = await _httpClient.GetFromJsonAsync<List<Local>>("");

                // Filtrar los locales por la categoría seleccionada
                var localesFiltrados = locales?.Where(l => l.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();

                return localesFiltrados ?? new List<Local>();
            }
            catch (Exception ex)
            {
                // Manejar errores de conexión o datos aquí
                Console.WriteLine($"Error al obtener locales: {ex.Message}");
                return new List<Local>();
            }
        }
    }

    public class Local
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Logo { get; set; }
        public string Categoria { get; set; }
    }
}
