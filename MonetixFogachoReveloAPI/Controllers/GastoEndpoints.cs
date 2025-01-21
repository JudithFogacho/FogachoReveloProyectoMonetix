using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace MonetixFogachoReveloAPI.Controllers
{
    public static class GastoEndpoints
    {
        public static void MapGastoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Gasto").WithTags(nameof(Gasto));

            // Obtener todos los gastos de un usuario
            // Obtener todos los gastos de un usuario
            group.MapGet("/usuario/{userId}", async (int userId, FogachoReveloDataContext db) =>
            {
                var usuario = await db.Usuarios.FindAsync(userId);
                if (usuario == null)
                {
                    return Results.NotFound();
                }

                var gastos = await db.Gastos
                    .Where(g => g.IdUsuario == userId)
                    .ToListAsync();

                return Results.Ok(gastos);
            })
            .WithName("GetGastosByUserId")
            .WithOpenApi();

            // Obtener un gasto por su ID
            group.MapGet("/{id}", async Task<Results<Ok<Gasto>, NotFound>> (int id, FogachoReveloDataContext db) =>
            {
                var gasto = await db.Gastos
                    .Include(g => g.Usuario)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.IdGasto == id);

                return gasto is not null ? TypedResults.Ok(gasto) : TypedResults.NotFound();
            })
            .WithName("GetGastoById")
            .WithOpenApi();

            // Actualizar un gasto por su ID
            group.MapPut("/{id}", async Task<Results<Ok, NotFound, BadRequest<string>>> (int id, Gasto gasto, FogachoReveloDataContext db) =>
            {
                var gastoExistente = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == gasto.IdUsuario);

                if (gastoExistente is null)
                    return TypedResults.NotFound();

                if (gastoExistente.IdUsuario != gasto.IdUsuario)
                    return TypedResults.BadRequest("No autorizado para modificar este gasto.");

                if (gasto.FechaRegristo != default)
                    gastoExistente.FechaRegristo = gasto.FechaRegristo;

                if (gasto.FechaFinal != default)
                    gastoExistente.FechaFinal = gasto.FechaFinal;

                if (gasto.Categorias != default(Categoria))
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
            group.MapPost("/", async Task<Results<Created<Gasto>, BadRequest<string>>> (Gasto gasto, FogachoReveloDataContext db) =>
            {
                if (gasto.IdUsuario <= 0)
                {
                    return TypedResults.BadRequest("Se requiere un ID de usuario válido.");
                }

                var usuario = await db.Usuarios.FindAsync(gasto.IdUsuario);
                if (usuario == null)
                {
                    return TypedResults.BadRequest("Usuario no encontrado.");
                }

                gasto.ValidarValor();
                db.Gastos.Add(gasto);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Gasto/{gasto.IdGasto}", gasto);
            })
            .WithName("CreateGasto")
            .WithOpenApi();

            // Eliminar un gasto
            group.MapDelete("/{id}/{userId}", async Task<Results<Ok, NotFound, BadRequest<string>>> (int id, int userId, FogachoReveloDataContext db) =>
            {
                var gasto = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gasto is null)
                    return TypedResults.NotFound();

                if (gasto.IdUsuario != userId)
                    return TypedResults.BadRequest("No autorizado para eliminar este gasto.");

                db.Gastos.Remove(gasto);
                await db.SaveChangesAsync();
                return TypedResults.Ok();
            })
            .WithName("DeleteGasto")
            .WithOpenApi();

            // Realizar pago a un gasto
            group.MapPut("/Pagar/{id}/{userId}", async Task<Results<Ok, NotFound, BadRequest<string>>> (int id, int userId, PagoRequest pagoRequest, FogachoReveloDataContext db) =>
            {
                var gastoExistente = await db.Gastos
                    .FirstOrDefaultAsync(g => g.IdGasto == id && g.IdUsuario == userId);

                if (gastoExistente is null)
                    return TypedResults.NotFound();

                if (gastoExistente.IdUsuario != userId)
                    return TypedResults.BadRequest("No autorizado para pagar este gasto.");

                if (pagoRequest.ValorPagado <= 0)
                    return TypedResults.BadRequest("El valor a pagar debe ser mayor que cero.");

                gastoExistente.ValorPagado += pagoRequest.ValorPagado;
                ActualizarEstado(gastoExistente);

                await db.SaveChangesAsync();
                return TypedResults.Ok();
            })
            .WithName("PagarGasto")
            .WithOpenApi();
        }

        // Método auxiliar para actualizar el estado
        private static void ActualizarEstado(Gasto gasto)
        {
            DateTime fechaActual = DateTime.Today;
            DateTime fechaFinalSinHora = gasto.FechaFinal.Date;

            if (gasto.Valor != null && gasto.ValorPagado >= gasto.Valor)
            {
                gasto.Estados = Estado.Finalizado;
            }
            else if (fechaActual > fechaFinalSinHora && gasto.Valor > 0)
            {
                gasto.Estados = Estado.Atrasado;
            }
            else if (fechaActual <= fechaFinalSinHora && gasto.Valor > 0)
            {
                gasto.Estados = Estado.Pendiente;
            }
        }

        // Clase para la solicitud de pago
        public class PagoRequest
        {
            public double ValorPagado { get; set; }
        }
    }
}