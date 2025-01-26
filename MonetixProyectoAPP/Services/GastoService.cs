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

    public async Task<List<GastoResponse>> GetGastosAsync(string? preference = null)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var url = $"gastos?userId={userId}";
            if (!string.IsNullOrEmpty(preference)) url += $"&preference={preference}";

            return await _httpClient.GetFromJsonAsync<List<GastoResponse>>(url)
                ?? new List<GastoResponse>();
        }
        catch (Exception)
        {
            return new List<GastoResponse>();
        }
    }

    public async Task<GastoResponse?> CreateGastoAsync(DateTime fechaRegistro, DateTime fechaFinal, string categoria,
        string descripcion, double valor)
    {
        var request = new CreateGastoRequest(
            _usuarioService.CurrentUserId,
            fechaRegistro,
            fechaFinal,
            categoria,
            descripcion,
            valor);

        var response = await _httpClient.PostAsJsonAsync("gastos", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GastoResponse>()
            : null;
    }

    public async Task<GastoResponse?> UpdateGastoAsync(int idGasto, DateTime? fechaRegistro = null, DateTime? fechaFinal = null,
        Categoria? categoria = null, string? descripcion = null, double? valor = null)
    {
        var request = new UpdateGastoRequest(fechaRegistro, fechaFinal, categoria, descripcion, valor);
        var response = await _httpClient.PutAsJsonAsync(
            $"gastos/{idGasto}?userId={_usuarioService.CurrentUserId}", request);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GastoResponse>()
            : null;
    }

    public async Task<GastoResponse?> PagarGastoAsync(int idGasto, double valorPago)
    {
        var request = new PagoRequest(valorPago);
        var response = await _httpClient.PostAsJsonAsync(
            $"gastos/{idGasto}/pagos?userId={_usuarioService.CurrentUserId}", request);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GastoResponse>()
            : null;
    }

    public async Task<ResumenGastos?> GetResumenGastosAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ResumenGastos>(
                $"gastos/resumen?userId={_usuarioService.CurrentUserId}");
        }
        catch
        {
            return null;
        }
    }
    public async Task<bool> DeleteGastoAsync(int idGasto)
    {
        try
        {
            var userId = _usuarioService.CurrentUserId;
            var response = await _httpClient.DeleteAsync($"gastos/{idGasto}?userId={userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Manejo de errores
            Console.WriteLine($"Error al eliminar el gasto: {ex.Message}");
            return false;
        }
    }
}

public record CreateGastoRequest(int UserId, DateTime FechaRegistro, DateTime FechaFinal, string Categoria,
    string Descripcion, double Valor);
public record UpdateGastoRequest(DateTime? FechaRegistro, DateTime? FechaFinal, Categoria? Categoria, string? Descripcion,
    double? Valor);
public record GastoResponse( int IdGasto, DateTime FechaRegistro, DateTime FechaFinal, Categoria? Categoria, 
    string Descripcion, double Valor, double ValorPagado, Estado Estado)
{
    // Propiedad calculada que devuelve el color correspondiente al estado
    public string ColorEstado
    {
        get
        {
            return Estado switch
            {
                Estado.Finalizado => "#81C784",
                Estado.Atrasado => "#E57373", // Rojo
                Estado.Pendiente => "#FFD54F", // Amarillo
                 // Verde
                _ => "#FFFFFF" // Blanco por defecto
            };
        }
    }
}
public record PagoRequest(double Valor);

public record ResumenGastos(
    int TotalGastos,
    int GastosPendientes,
    int GastosAtrasados,
    int GastosFinalizados,
    double ValorTotal,
    double ValorPagado,
    double ValorPendiente);
