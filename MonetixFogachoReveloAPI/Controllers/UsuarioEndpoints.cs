using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Controllers
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Usuario").WithTags(nameof(Usuario));

            // Endpoint para obtener todos los usuarios
            group.MapGet("/", async (FogachoReveloDataContext db) =>
            {
                return await db.Usuarios.ToListAsync();
            })
            .WithName("GetAllUsuarios")
            .WithOpenApi();

            // Endpoint para obtener un usuario por ID
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

            // Endpoint para actualizar un usuario
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

            // Endpoint para crear un usuario
            group.MapPost("/", async (Usuario usuario, FogachoReveloDataContext db) =>
            {
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Usuario/{usuario.IdUsuario}", usuario);
            })
            .WithName("CreateUsuario")
            .WithOpenApi();

            // Endpoint para eliminar un usuario
            group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int idusuario, FogachoReveloDataContext db) =>
            {
                var affected = await db.Usuarios
                    .Where(model => model.IdUsuario == idusuario)
                    .ExecuteDeleteAsync();
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("DeleteUsuario")
            .WithOpenApi();

            group.MapPost("/login", async Task<Results<Ok<Usuario>, BadRequest<string>>> (Usuario usuario, FogachoReveloDataContext db) =>
            {
                if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Password))
                {
                    return TypedResults.BadRequest("Email y contraseña son requeridos.");
                }

                var usuarioEncontrado = await db.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == usuario.Email);

                if (usuarioEncontrado == null)
                {
                    return TypedResults.BadRequest("Email no encontrado.");
                }

                if (usuarioEncontrado.Password != usuario.Password)
                {
                    return TypedResults.BadRequest("Contraseña incorrecta.");
                }

                return TypedResults.Ok(usuarioEncontrado);
            })
            .WithName("Login")
            .WithOpenApi();

        }
    }
}
