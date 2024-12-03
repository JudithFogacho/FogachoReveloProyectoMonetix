using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;
namespace MonetixFogachoReveloAPI.Controllers;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Usuario").WithTags(nameof(Usuario));

        group.MapGet("/", async (FogachoReveloDataContext db) =>
        {
            return await db.Usuarios.ToListAsync();
        })
        .WithName("GetAllUsuarios")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Usuario>, NotFound>> (int idusuario, FogachoReveloDataContext db) =>
        {
            return await db.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(model => model.IdUsuario == idusuario)
                is Usuario model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetUsuarioById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int idusuario, Usuario usuario, FogachoReveloDataContext db) =>
        {
            var affected = await db.Usuarios
                .Where(model => model.IdUsuario == idusuario)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.IdUsuario, usuario.IdUsuario)
                    .SetProperty(m => m.Nombre, usuario.Nombre)
                    .SetProperty(m => m.Apellido, usuario.Apellido)
                    .SetProperty(m => m.Email, usuario.Email)
                    .SetProperty(m => m.Password, usuario.Password)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateUsuario")
        .WithOpenApi();

        group.MapPost("/", async (Usuario usuario, FogachoReveloDataContext db) =>
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Usuario/{usuario.IdUsuario}",usuario);
        })
        .WithName("CreateUsuario")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int idusuario, FogachoReveloDataContext db) =>
        {
            var affected = await db.Usuarios
                .Where(model => model.IdUsuario == idusuario)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteUsuario")
        .WithOpenApi();
    }
}
