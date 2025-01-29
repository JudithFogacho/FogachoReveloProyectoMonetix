using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Controllers;

public static class GastoEndpoints
{
    public static void MapGastoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/gastos");

        group.MapGet("/", async (int userId, string? preference, FogachoReveloDataContext db) =>
        {
            var query = db.Gastos
                .Include(g => g.Usuario)
                .Where(g => g.IdUsuario == userId);

            if (!string.IsNullOrEmpty(preference))
            {
                query = preference.ToLower() switch
                {
                    "pendientes" => query.Where(g => g.Estados == Estado.Pendiente),
                    "atrasados" => query.Where(g => g.Estados == Estado.Atrasado),
                    "finalizados" => query.Where(g => g.Estados == Estado.Finalizado),
                    _ => query
                };
            }

            var gastos = await query
                .OrderByDescending(g => g.FechaRegristo)
                .Select(g => new GastoWithUserResponse
                {
                    IdGasto = g.IdGasto,
                    FechaRegristo = g.FechaRegristo,
                    FechaFinal = g.FechaFinal,
                    Categorias = (int)g.Categorias,
                    Descripcion = g.Descripcion,
                    Valor = g.Valor ?? 0,
                    ValorPagado = g.ValorPagado,
                    Estados = (int)g.Estados,
                    IdUsuario = g.IdUsuario,
                    Usuario = g.Usuario != null ? new UsuarioResponse
                    {
                        IdUsuario = g.Usuario.IdUsuario,
                        Nombre = g.Usuario.Nombre,
                        Apellido = g.Usuario.Apellido,
                        Email = g.Usuario.Email
                    } : null
                })
                .ToListAsync();

            return Results.Ok(gastos);
        });

        group.MapPost("/", async (CreateGastoRequest request, FogachoReveloDataContext db) =>
        {
            // Validar y convertir la categoría a su valor numérico
            if (!Enum.TryParse<Categoria>(request.Categoria, true, out Categoria categoria))
            {
                return Results.BadRequest("Categoría inválida");
            }

            var gasto = new Gasto
            {
                IdUsuario = request.UserId,
                FechaRegristo = DateTime.UtcNow,
                FechaFinal = request.FechaFinal,
                Categorias = categoria,
                Descripcion = request.Descripcion,
                Valor = request.Valor,
                ValorPagado = 0,
                Estados = Estado.Pendiente
            };

            db.Gastos.Add(gasto);
            await db.SaveChangesAsync();
            return Results.Created($"/api/gastos/{gasto.IdGasto}", gasto);
        });

        group.MapPut("/{id}", async (int id, int userId, UpdateGastoRequest request, FogachoReveloDataContext db) =>
        {
            var gasto = await db.Gastos.FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);
            if (gasto == null) return Results.NotFound();

            if (request.Categoria != null)
            {
                if (!Enum.TryParse<Categoria>(request.Categoria, true, out Categoria categoria))
                {
                    return Results.BadRequest("Categoría inválida");
                }
                gasto.Categorias = categoria;
            }

            if (request.FechaFinal.HasValue)
                gasto.FechaFinal = request.FechaFinal.Value;
            if (request.Descripcion != null)
                gasto.Descripcion = request.Descripcion;
            if (request.Valor.HasValue)
                gasto.Valor = request.Valor.Value;

            ActualizarEstado(gasto);
            await db.SaveChangesAsync();
            return Results.Ok(gasto);
        });

        group.MapPost("/{id}/pagos", async (int id, int userId, PagoRequest request, FogachoReveloDataContext db) =>
        {
            var gasto = await db.Gastos.FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);
            if (gasto == null) return Results.NotFound();

            gasto.ValorPagado += request.Valor;
            ActualizarEstado(gasto);
            await db.SaveChangesAsync();
            return Results.Ok(gasto);
        });

        group.MapGet("/resumen", async (int userId, FogachoReveloDataContext db) =>
        {
            var gastos = await db.Gastos.Where(g => g.IdUsuario == userId).ToListAsync();

            return Results.Ok(new
            {
                TotalGastos = gastos.Count,
                GastosPendientes = gastos.Count(g => g.Estados == Estado.Pendiente),
                GastosAtrasados = gastos.Count(g => g.Estados == Estado.Atrasado),
                GastosFinalizados = gastos.Count(g => g.Estados == Estado.Finalizado),
                ValorTotal = gastos.Sum(g => g.Valor ?? 0),
                ValorPagado = gastos.Sum(g => g.ValorPagado),
                ValorPendiente = gastos.Sum(g => (g.Valor ?? 0) - g.ValorPagado)
            });
        });
    }

    private static void ActualizarEstado(Gasto gasto)
    {
        if (gasto.ValorPagado >= gasto.Valor)
            gasto.Estados = Estado.Finalizado;
        else if (DateTime.Today > gasto.FechaFinal.Date)
            gasto.Estados = Estado.Atrasado;
        else
            gasto.Estados = Estado.Pendiente;
    }
}

// DTOs
public record CreateGastoRequest(int UserId, DateTime FechaFinal, string Categoria, string Descripcion, double Valor);
public record UpdateGastoRequest(DateTime? FechaFinal = null, string? Categoria = null, string? Descripcion = null, double? Valor = null);
public record PagoRequest(double Valor);
public class GastoWithUserResponse
{
    public int IdGasto { get; set; }
    public DateTime FechaRegristo { get; set; }
    public DateTime FechaFinal { get; set; }
    public int Categorias { get; set; }
    public string Descripcion { get; set; }
    public double Valor { get; set; }
    public double ValorPagado { get; set; }
    public int Estados { get; set; }
    public int IdUsuario { get; set; }
    public UsuarioResponse Usuario { get; set; }
}

public class UsuarioResponse
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
}