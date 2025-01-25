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

    public async Task<GastoResponse?> CreateGastoAsync(DateTime fechaFinal, string categoria,
        string descripcion, double valor)
    {
        var request = new CreateGastoRequest(
            _usuarioService.CurrentUserId,
            fechaFinal,
            categoria,
            descripcion,
            valor);

        var response = await _httpClient.PostAsJsonAsync("gastos", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GastoResponse>()
            : null;
    }

    public async Task<GastoResponse?> UpdateGastoAsync(int idGasto, DateTime? fechaFinal = null,
        string? categoria = null, string? descripcion = null, double? valor = null)
    {
        var request = new UpdateGastoRequest(fechaFinal, categoria, descripcion, valor);
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
        catch
        {
            return false;
        }
    }
}

public record CreateGastoRequest(int UserId, DateTime FechaFinal, string Categoria,
    string Descripcion, double Valor);
public record UpdateGastoRequest(DateTime? FechaFinal, string? Categoria, string? Descripcion,
    double? Valor);
public record GastoResponse(int IdGasto, DateTime FechaRegistro, DateTime FechaFinal,
    string Categoria, string Descripcion, double Valor, double ValorPagado, string Estado)
{
    public string ConvertirEstado(string estado)
    {
        return estado.ToLower() switch
        {
            "atrasado" => "Gasto atrasado",
            "pendiente" => "Gasto pendiente",
            "finalizado" => "Gasto finalizado",
            _ => "Estado desconocido"
        };
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