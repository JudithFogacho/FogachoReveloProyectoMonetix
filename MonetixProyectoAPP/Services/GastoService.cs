using MonetixProyectoAPP.Models;
using System.Net.Http.Json;

namespace MonetixProyectoAPP.Services;

public class GastoService
{
    private readonly HttpClient _httpClient;
    private readonly UsuarioService _usuarioService;

    public GastoService(UsuarioService usuarioService)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7156/api/") };
        _usuarioService = usuarioService;
    }

    public async Task<List<Gasto>> GetGastosAsync(string? preference = null)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var url = $"gastos?userId={userId}";
            if (!string.IsNullOrEmpty(preference)) url += $"&preference={preference}";

            var response = await _httpClient.GetFromJsonAsync<List<GastoResponse>>(url);
            return response?.Select(MapGastoResponseToGasto).ToList() ?? new List<Gasto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo gastos: {ex.Message}");
            return new List<Gasto>();
        }
    }

    public async Task<Gasto?> CreateGastoAsync(DateTime fechaFinal, Categoria categoria,
        string empresa, string descripcion, double valor)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var descripcionCompleta = $"{empresa} - {descripcion}";

            var request = new CreateGastoRequest(
                userId,
                fechaFinal,
                categoria.ToString(),
                descripcionCompleta,
                valor);

            var response = await _httpClient.PostAsJsonAsync("gastos", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear gasto: {error}");
                return null;
            }

            var gastoResponse = await response.Content.ReadFromJsonAsync<GastoResponse>();
            return gastoResponse != null ? MapGastoResponseToGasto(gastoResponse) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear gasto: {ex.Message}");
            return null;
        }
    }

    public async Task<Gasto?> UpdateGastoAsync(int idGasto, DateTime? fechaFinal = null,
        Categoria? categoria = null, string? descripcion = null, double? valor = null)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var request = new UpdateGastoRequest(
                fechaFinal,
                categoria?.ToString(),
                descripcion,
                valor);

            var response = await _httpClient.PutAsJsonAsync($"gastos/{idGasto}?userId={userId}", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al actualizar gasto: {error}");
                return null;
            }

            var gastoResponse = await response.Content.ReadFromJsonAsync<GastoResponse>();
            return gastoResponse != null ? MapGastoResponseToGasto(gastoResponse) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar gasto: {ex.Message}");
            return null;
        }
    }

    public async Task<Gasto?> PagarGastoAsync(int idGasto, double valorPago)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var request = new PagoRequest(valorPago);

            var response = await _httpClient.PostAsJsonAsync(
                $"gastos/{idGasto}/pagos?userId={userId}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al realizar pago: {error}");
                return null;
            }

            var gastoResponse = await response.Content.ReadFromJsonAsync<GastoResponse>();
            return gastoResponse != null ? MapGastoResponseToGasto(gastoResponse) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al realizar pago: {ex.Message}");
            return null;
        }
    }

    public async Task<ResumenGastos?> GetResumenGastosAsync()
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            return await _httpClient.GetFromJsonAsync<ResumenGastos>(
                $"gastos/resumen?userId={userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener resumen: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteGastoAsync(int idGasto)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var response = await _httpClient.DeleteAsync($"gastos/{idGasto}?userId={userId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al eliminar gasto: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el gasto: {ex.Message}");
            return false;
        }
    }

    private static Gasto MapGastoResponseToGasto(GastoResponse response)
    {
        var gasto = new Gasto
        {
            IdGasto = response.IdGasto,
            FechaRegristo = response.FechaRegristo,
            FechaFinal = response.FechaFinal,
            Categorias = (Categoria)response.Categorias,
            Descripcion = response.Descripcion,
            Valor = response.Valor,
            ValorPagado = response.ValorPagado,
            Estados = (Estado)response.Estados,
            IdUsuario = response.IdUsuario
        };

        if (response.Usuario != null)
        {
            gasto.Usuario = new Usuario
            {
                IdUsuario = response.Usuario.IdUsuario,
                Nombre = response.Usuario.Nombre,
                Apellido = response.Usuario.Apellido,
                Email = response.Usuario.Email
            };
        }

        gasto.AsignarColorEstado();
        return gasto;
    }
}