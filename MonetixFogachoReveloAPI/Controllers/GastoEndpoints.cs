using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace MonetixFogachoReveloAPI.Controllers;
public static class GastoEndpoints
{
    public static void MapGastoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Gasto").WithTags(nameof(Gasto));

        // Obtener todos los gastos
        group.MapGet("/", async (FogachoReveloDataContext db) =>
        {
            return await db.Gastos.ToListAsync();
        })
        .WithName("GetAllGastos")
        .WithOpenApi();

        // Obtener un gasto por su ID
        group.MapGet("/{id}", async Task<Results<Ok<Gasto>, NotFound>> (int id, FogachoReveloDataContext db) =>
        {
            var gasto = await db.Gastos.AsNoTracking().FirstOrDefaultAsync(g => g.IdGasto == id);
            return gasto is not null ? TypedResults.Ok(gasto) : TypedResults.NotFound();
        })
        .WithName("GetGastoById")
        .WithOpenApi();

        // Actualizar un gasto por su ID
        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Gasto gasto, FogachoReveloDataContext db) =>
        {
            var gastoExistente = await db.Gastos.FirstOrDefaultAsync(g => g.IdGasto == id);

            if (gastoExistente is null)
                return TypedResults.NotFound();

            // Actualizar solo las propiedades no nulas o con valores válidos
            if (gasto.FechaRegristo != default)
                gastoExistente.FechaRegristo = gasto.FechaRegristo;

            if (gasto.FechaFinal != default)
                gastoExistente.FechaFinal = gasto.FechaFinal;

            if (gasto.Categorias != default(Categoria)) // Verifica si se proporcionó un valor válido
                gastoExistente.Categorias = gasto.Categorias;

            if (!string.IsNullOrWhiteSpace(gasto.Descripcion))
                gastoExistente.Descripcion = gasto.Descripcion;

            if (gasto.Valor > 0)
                gastoExistente.Valor = gasto.Valor;

            if (gasto.ValorPagado > 0)
            {
                gastoExistente.ValorPagado = gasto.ValorPagado;
                ActualizarEstado(gastoExistente);
            }

            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("UpdateGasto")
        .WithOpenApi();

        // Crear un nuevo gasto
        group.MapPost("/", async (Gasto gasto, FogachoReveloDataContext db) =>
        {
            db.Gastos.Add(gasto);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Gasto/{gasto.IdGasto}", gasto);
        })
        .WithName("CreateGasto")
        .WithOpenApi();

        // Eliminar un gasto por su ID
        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, FogachoReveloDataContext db) =>
        {
            var gasto = await db.Gastos.FindAsync(id);

            if (gasto is null)
                return TypedResults.NotFound();

            db.Gastos.Remove(gasto);
            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("DeleteGasto")
        .WithOpenApi();
    }

    // Método auxiliar para actualizar el estado
    private static void ActualizarEstado(Gasto gasto)
    {
        DateTime fechaActual = DateTime.Today;
        DateTime fechaFinalSinHora = gasto.FechaFinal.Date;

        // Si ya está todo pagado el estado será finalizado
        if (gasto.Valor != null && gasto.ValorPagado == gasto.Valor)
        {
            gasto.Estados = Estado.Finalizado;
        }
        // Si la fecha se ha pasado de la fecha final y no se ha pagado será Atrasado
        else if (fechaActual > fechaFinalSinHora && gasto.Valor > 0)
        {
            gasto.Estados = Estado.Atrasado;
        }
        // Si la fecha aun no ha pasado la fecha final y no está pagado será Pendiente
        else if (fechaActual <= fechaFinalSinHora && gasto.Valor > 0)
        {
            gasto.Estados = Estado.Pendiente;
        }
    }
}
