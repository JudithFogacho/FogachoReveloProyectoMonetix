using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Controllers;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios");

        group.MapPost("/register", async (CreateUsuarioRequest request, FogachoReveloDataContext db) =>
        {
            if (await db.Usuarios.AnyAsync(u => u.Email == request.Email))
                return Results.BadRequest("Email ya registrado");

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Password = request.Password
            };

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            return Results.Created($"/api/usuarios/{usuario.IdUsuario}",
                new { usuario.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email });
        });

        group.MapPost("/login", async (LoginRequest request, FogachoReveloDataContext db) =>
        {
            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

            return usuario == null
                ? Results.BadRequest("Credenciales inválidas")
                : Results.Ok(new { usuario.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email });
        });

        group.MapGet("/{id}", async (int id, FogachoReveloDataContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            return usuario == null
                ? Results.NotFound()
                : Results.Ok(new { usuario.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email });
        });
    }
}

public record CreateUsuarioRequest(string Nombre, string Apellido, string Email, string Password);
public record LoginRequest(string Email, string Password);