using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using MonetixProyectoAPP.Services;

namespace MonetixProyectoAPP.Services
{
    public class GastoService
    {
        private readonly HttpClient _httpClient;
        private readonly UsuarioService _usuarioService;

        public GastoService(UsuarioService usuarioService)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7156/api/")
            };
            _usuarioService = usuarioService;
        }

        public async Task<List<GastoResponse>> GetGastosAsync()
        {
            try
            {
                var userId = _usuarioService.CurrentUserId;
                return await _httpClient.GetFromJsonAsync<List<GastoResponse>>($"gastos?userId={userId}")
                    ?? new List<GastoResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los gastos: {ex.Message}");
                return new List<GastoResponse>();
            }
        }

        // En GastoService.cs (MAUI)
        public async Task<GastoResponse> CreateGastoAsync(
            DateTime fechaFinal,
            string categoria,
            string descripcion,
            double valor)
        {
            try
            {
                // Validar usuario autenticado
                if (_usuarioService.CurrentUserId <= 0)
                    throw new InvalidOperationException("Usuario no autenticado");

                // Crear request con userId
                var request = new CreateGastoRequest(
                    UserId: _usuarioService.CurrentUserId,
                    FechaFinal: fechaFinal,
                    Categoria: categoria,
                    Descripcion: descripcion,
                    Valor: valor
                );

                // Enviar solicitud a la API
                var response = await _httpClient.PostAsJsonAsync("gastos", request);

                // Manejar errores HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
                }

                // Leer y retornar respuesta
                var gastoCreado = await response.Content.ReadFromJsonAsync<GastoResponse>();
                return gastoCreado!;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CreateGastoAsync: {ex}");
                throw; // Relanzar para manejo en el ViewModel
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
                Console.WriteLine($"Error al eliminar el gasto: {ex.Message}");
                return false;
            }
        }

        public async Task<GastoResponse> PagarGastoAsync(int idGasto, double valorPago)
        {
            try
            {
                var pagoRequest = new PagoRequest(
                    UserId: _usuarioService.CurrentUserId,
                    Valor: valorPago
                );

                var response = await _httpClient.PostAsJsonAsync($"gastos/{idGasto}/pagos", pagoRequest);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GastoResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al realizar el pago: {ex.Message}");
                throw;
            }
        }

        public async Task<GastoResponse> GetGastoByIdAsync(int idGasto)
        {
            try
            {
                var userId = _usuarioService.CurrentUserId;
                return await _httpClient.GetFromJsonAsync<GastoResponse>($"gastos/{idGasto}?userId={userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el gasto: {ex.Message}");
                throw;
            }
        }

        public async Task<GastoResponse> UpdateGastoAsync(int idGasto, DateTime? fechaFinal, string? categoria, string? descripcion, double? valor)
        {
            try
            {
                var request = new UpdateGastoRequest(
                    UserId: _usuarioService.CurrentUserId,
                    FechaFinal: fechaFinal,
                    Categoria: categoria,
                    Descripcion: descripcion,
                    Valor: valor
                );

                var response = await _httpClient.PutAsJsonAsync($"gastos/{idGasto}", request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GastoResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el gasto: {ex.Message}");
                throw;
            }
        }

        public async Task<ResumenGastos> GetResumenGastosAsync()
        {
            try
            {
                var userId = _usuarioService.CurrentUserId;
                return await _httpClient.GetFromJsonAsync<ResumenGastos>($"gastos/resumen?userId={userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el resumen: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs
    public record CreateGastoRequest(
        int UserId,
        DateTime FechaFinal,
        string Categoria,
        string Descripcion,
        double Valor);

    public record UpdateGastoRequest(
        int UserId,
        DateTime? FechaFinal = null,
        string? Categoria = null,
        string? Descripcion = null,
        double? Valor = null);

    public record GastoResponse(
        int IdGasto,
        DateTime FechaRegistro,
        DateTime FechaFinal,
        string Categoria,
        string Descripcion,
        double Valor,
        double ValorPagado,
        string Estado);

    public record PagoRequest(
        int UserId,
        double Valor);

    public class ResumenGastos
    {
        public int TotalGastos { get; set; }
        public int GastosPendientes { get; set; }
        public int GastosAtrasados { get; set; }
        public int GastosFinalizados { get; set; }
        public double ValorTotal { get; set; }
        public double ValorPagado { get; set; }
        public double ValorPendiente { get; set; }
    }
}