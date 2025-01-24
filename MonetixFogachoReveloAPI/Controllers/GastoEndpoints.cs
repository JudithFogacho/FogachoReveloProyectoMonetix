using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Controllers
{
    public static class GastoEndpoints
    {
        public static void MapGastoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/gastos")
                .WithTags("Gastos")
                .RequireAuthorization()
                .WithOpenApi();

            // GET: Obtener todos los gastos del usuario
            group.MapGet("/", async Task<Results<Ok<List<GastoResponse>>, UnauthorizedHttpResult>> (
                ClaimsPrincipal user,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gastos = await db.Gastos
                    .Where(g => g.IdUsuario == userId)
                    .OrderByDescending(g => g.FechaRegristo)
                    .Select(g => MapToResponse(g))
                    .ToListAsync();

                return TypedResults.Ok(gastos);
            })
            .WithName("GetAllGastos")
            .WithSummary("Obtiene todos los gastos del usuario autenticado")
            .Produces<List<GastoResponse>>(200)
            .Produces(401);

            // GET: Obtener gasto por ID
            group.MapGet("/{id}", async Task<Results<Ok<GastoResponse>, NotFound, UnauthorizedHttpResult>> (
                int id,
                ClaimsPrincipal user,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gasto = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gasto == null) return TypedResults.NotFound();

                return TypedResults.Ok(MapToResponse(gasto));
            })
            .WithName("GetGastoById")
            .WithSummary("Obtiene un gasto específico por su ID")
            .Produces<GastoResponse>(200)
            .Produces(404)
            .Produces(401);

            // POST: Crear nuevo gasto
            // POST: Crear nuevo gasto
            group.MapPost("/", async Task<Results<Created<GastoResponse>, BadRequest<string>>> (
                CreateGastoRequest request,
                FogachoReveloDataContext db) =>
            {
                // Validaciones
                if (request.UserId <= 0)
                    return TypedResults.BadRequest("UserId inválido");

                if (!Enum.TryParse<Categoria>(request.Categoria, out var categoriaEnum))
                    return TypedResults.BadRequest("Categoría inválida");

                var usuario = await db.Usuarios.FindAsync(request.UserId);
                if (usuario == null)
                    return TypedResults.BadRequest("Usuario no existe");

                var gasto = new Gasto
                {
                    IdUsuario = request.UserId,
                    FechaRegristo = DateTime.UtcNow,
                    FechaFinal = request.FechaFinal,
                    Categorias = categoriaEnum,
                    Descripcion = request.Descripcion,
                    Valor = request.Valor,
                    ValorPagado = 0,
                    Estados = Estado.Pendiente
                };

                try
                {
                    db.Gastos.Add(gasto);
                    await db.SaveChangesAsync();
                    return TypedResults.Created($"/api/gastos/{gasto.IdGasto}", MapToResponse(gasto));
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error: {ex.Message}");
                }
            });

            // PUT: Actualizar gasto
            group.MapPut("/{id}", async Task<Results<Ok<GastoResponse>, NotFound, BadRequest<string>, UnauthorizedHttpResult>> (
                int id,
                ClaimsPrincipal user,
                UpdateGastoRequest request,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gasto = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gasto == null) return TypedResults.NotFound();

                if (!ValidateUpdateRequest(request, out var errorMessage))
                    return TypedResults.BadRequest(errorMessage);

                try
                {
                    UpdateGasto(gasto, request);
                    await db.SaveChangesAsync();
                    return TypedResults.Ok(MapToResponse(gasto));
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error al actualizar el gasto: {ex.Message}");
                }
            })
            .WithName("UpdateGasto")
            .WithSummary("Actualiza un gasto existente")
            .Produces<GastoResponse>(200)
            .Produces(404)
            .Produces<string>(400)
            .Produces(401);

            // DELETE: Eliminar gasto
            group.MapDelete("/{id}", async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> (
                int id,
                ClaimsPrincipal user,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gasto = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gasto == null) return TypedResults.NotFound();

                db.Gastos.Remove(gasto);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            })
            .WithName("DeleteGasto")
            .WithSummary("Elimina un gasto")
            .Produces(204)
            .Produces(404)
            .Produces(401);

            // POST: Realizar pago
            group.MapPost("/{id}/pagos", async Task<Results<Ok<GastoResponse>, NotFound, BadRequest<string>, UnauthorizedHttpResult>> (
                int id,
                ClaimsPrincipal user,
                PagoRequest request,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gasto = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gasto == null) return TypedResults.NotFound();

                if (request.Valor <= 0)
                    return TypedResults.BadRequest("El valor del pago debe ser positivo");

                if (gasto.ValorPagado + request.Valor > gasto.Valor)
                    return TypedResults.BadRequest("El pago excede el valor pendiente del gasto");

                try
                {
                    gasto.ValorPagado += request.Valor;
                    ActualizarEstado(gasto);
                    await db.SaveChangesAsync();
                    return TypedResults.Ok(MapToResponse(gasto));
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error al procesar el pago: {ex.Message}");
                }
            })
            .WithName("AddPayment")
            .WithSummary("Registra un pago para un gasto")
            .Produces<GastoResponse>(200)
            .Produces(404)
            .Produces<string>(400)
            .Produces(401);

            // GET: Obtener resumen de gastos
            group.MapGet("/resumen", async Task<Results<Ok<ResumenGastos>, UnauthorizedHttpResult>> (
                ClaimsPrincipal user,
                FogachoReveloDataContext db) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return TypedResults.Unauthorized();

                var gastos = await db.Gastos
                    .Where(g => g.IdUsuario == userId)
                    .ToListAsync();

                var resumen = new ResumenGastos
                {
                    TotalGastos = gastos.Count,
                    GastosPendientes = gastos.Count(g => g.Estados == Estado.Pendiente),
                    GastosAtrasados = gastos.Count(g => g.Estados == Estado.Atrasado),
                    GastosFinalizados = gastos.Count(g => g.Estados == Estado.Finalizado),
                    ValorTotal = gastos.Sum(g => g.Valor ?? 0),
                    ValorPagado = gastos.Sum(g => g.ValorPagado),
                    ValorPendiente = gastos.Sum(g => (g.Valor ?? 0) - g.ValorPagado)
                };

                return TypedResults.Ok(resumen);
            })
            .WithName("GetGastosResumen")
            .WithSummary("Obtiene un resumen de los gastos del usuario")
            .Produces<ResumenGastos>(200)
            .Produces(401);
        }

        private static GastoResponse MapToResponse(Gasto gasto) => new(
            gasto.IdGasto,
            gasto.FechaRegristo,
            gasto.FechaFinal,
            gasto.Categorias.ToString(),
            gasto.Descripcion ?? string.Empty,
            gasto.Valor ?? 0,
            gasto.ValorPagado,
            gasto.Estados.ToString());

        private static void ActualizarEstado(Gasto gasto)
        {
            var hoy = DateTime.Today;
            var fechaFinal = gasto.FechaFinal.Date;

            if (gasto.ValorPagado >= gasto.Valor)
                gasto.Estados = Estado.Finalizado;
            else if (hoy > fechaFinal)
                gasto.Estados = Estado.Atrasado;
            else
                gasto.Estados = Estado.Pendiente;
        }

        private static void UpdateGasto(Gasto gasto, UpdateGastoRequest request)
        {
            if (!string.IsNullOrEmpty(request.Categoria))
                gasto.Categorias = Enum.Parse<Categoria>(request.Categoria);

            if (request.FechaFinal.HasValue)
                gasto.FechaFinal = request.FechaFinal.Value;

            if (!string.IsNullOrEmpty(request.Descripcion))
                gasto.Descripcion = request.Descripcion;

            if (request.Valor.HasValue)
                gasto.Valor = request.Valor.Value;

            ActualizarEstado(gasto);
        }

        private static bool ValidateCreateRequest(CreateGastoRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (request.FechaFinal < DateTime.Today)
            {
                errorMessage = "La fecha final no puede ser anterior a hoy";
                return false;
            }

            if (!Enum.TryParse<Categoria>(request.Categoria, out _))
            {
                errorMessage = $"Categoría inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Categoria>())}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                errorMessage = "La descripción es requerida";
                return false;
            }

            if (request.Valor <= 0)
            {
                errorMessage = "El valor debe ser mayor que cero";
                return false;
            }

            return true;
        }

        private static bool ValidateUpdateRequest(UpdateGastoRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (request.FechaFinal.HasValue && request.FechaFinal.Value < DateTime.Today)
            {
                errorMessage = "La fecha final no puede ser anterior a hoy";
                return false;
            }

            if (!string.IsNullOrEmpty(request.Categoria) && !Enum.TryParse<Categoria>(request.Categoria, out _))
            {
                errorMessage = $"Categoría inválida. Valores permitidos: {string.Join(", ", Enum.GetNames<Categoria>())}";
                return false;
            }

            if (request.Valor.HasValue && request.Valor.Value <= 0)
            {
                errorMessage = "El valor debe ser mayor que cero";
                return false;
            }

            return true;
        }

        // DTOs
        public record CreateGastoRequest(
            int UserId,
            DateTime FechaFinal,
            string Categoria,
            string Descripcion,
            double Valor);

        public record UpdateGastoRequest(
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

        public record PagoRequest(double Valor);

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
}