using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;
namespace MonetixFogachoReveloAPI.Controllers;

public static class GastoEndpoints
{
    public static void MapGastoEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Gasto").WithTags(nameof(Gasto));

        group.MapGet("/", async (FogachoReveloDataContext db) =>
        {
            return await db.Gastos.ToListAsync();
        })
        .WithName("GetAllGastos")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Gasto>, NotFound>> (int idgasto, FogachoReveloDataContext db) =>
        {
            return await db.Gastos.AsNoTracking()
                .FirstOrDefaultAsync(model => model.IdGasto == idgasto)
                is Gasto model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetGastoById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int idgasto, Gasto gasto, FogachoReveloDataContext db) =>
        {
            var affected = await db.Gastos
            .Where(model => model.IdGasto == idgasto)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IdGasto, gasto.IdGasto)
                .SetProperty(m => m.FechaRegristo, gasto.FechaRegristo)
                .SetProperty(m => m.FechaFinal, gasto.FechaFinal)
                .SetProperty(m => m.Categorias, gasto.Categorias)
                .SetProperty(m => m.Descripcion, gasto.Descripcion)
                .SetProperty(m => m.Valor, gasto.Valor)
                .SetProperty(m => m.ValorPagado, gasto.ValorPagado)
                .SetProperty(m => m.Estados, gasto.Estados)
            );
            // Buscar el gasto existente en la base de datos
            var gastoExistente = await db.Gastos.FirstOrDefaultAsync(g => g.IdGasto == idgasto);

            if (gastoExistente == null)
            {
                return TypedResults.NotFound(); // Si no existe, responde con NotFound
            }

            // Actualiza los campos necesarios, como el valor pagado y el estado
            gastoExistente.ValorPagado = gasto.ValorPagado; // Actualiza el valor pagado
            gastoExistente.Estados = gasto.Estados; // Actualiza el estado (ej. "Pagado")

            // Si quieres también registrar la fecha de pago, puedes hacerlo
            gastoExistente.FechaFinal = gasto.FechaFinal; // Registra la fecha final como la fecha de pago

            // Guarda los cambios
            await db.SaveChangesAsync();

            return TypedResults.Ok(); // Responde con Ok si la actualización fue exitosa
        })
        .WithName("UpdateGasto")
        .WithOpenApi();

        group.MapPost("/", async (Gasto gasto, FogachoReveloDataContext db) =>
        {
            db.Gastos.Add(gasto);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Gasto/{gasto.IdGasto}",gasto);
        })
        .WithName("CreateGasto")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int idgasto, FogachoReveloDataContext db) =>
        {
            var affected = await db.Gastos
                .Where(model => model.IdGasto == idgasto)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteGasto")
        .WithOpenApi();
    }
}
