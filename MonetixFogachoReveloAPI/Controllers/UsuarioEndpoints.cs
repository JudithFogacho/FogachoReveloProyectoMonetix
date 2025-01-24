using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Controllers
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/usuarios")
                .WithTags("Usuarios")
                .WithOpenApi();

            // POST: Registrar nuevo usuario
            group.MapPost("/register", async Task<Results<Created<UsuarioResponse>, BadRequest<ErrorResponse>>> (
                [FromBody] CreateUsuarioRequest request,
                FogachoReveloDataContext db) =>
            {
                // Validación de modelo
                if (!Validator.TryValidateObject(request, new ValidationContext(request), null, true))
                    return TypedResults.BadRequest(new ErrorResponse("Datos inválidos"));

                // Verificar email único
                if (await db.Usuarios.AnyAsync(u => u.Email == request.Email))
                    return TypedResults.BadRequest(new ErrorResponse("Email ya registrado"));

                var usuario = new Usuario
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    Password = request.Password
                };

                try
                {
                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync();

                    return TypedResults.Created($"/api/usuarios/{usuario.IdUsuario}",
                        new UsuarioResponse(
                            usuario.IdUsuario,
                            usuario.Nombre,
                            usuario.Apellido,
                            usuario.Email));
                }
                catch (DbUpdateException ex)
                {
                    return TypedResults.BadRequest(new ErrorResponse($"Error de base de datos: {ex.InnerException?.Message}"));
                }
            })
            .WithName("RegisterUsuario")
            .WithSummary("Registra un nuevo usuario")
            .Produces<UsuarioResponse>(201)
            .Produces<ErrorResponse>(400);

            // POST: Login de usuario
            group.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<ErrorResponse>>> (
                [FromBody] LoginRequest request,
                FogachoReveloDataContext db) =>
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return TypedResults.BadRequest(new ErrorResponse("Email y contraseña requeridos"));

                // Buscar usuario
                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

                if (usuario == null)
                    return TypedResults.BadRequest(new ErrorResponse("Credenciales inválidas"));

                // Devolver datos necesarios para el cliente
                return TypedResults.Ok(new LoginResponse(
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email));
            })
            .WithName("LoginUsuario")
            .WithSummary("Inicia sesión con un usuario existente")
            .Produces<LoginResponse>(200)
            .Produces<ErrorResponse>(400);

            // GET: Obtener usuario por ID
            group.MapGet("/{id}", async Task<Results<Ok<UsuarioResponse>, NotFound, BadRequest<ErrorResponse>>> (
                int id,
                FogachoReveloDataContext db) =>
            {
                var usuario = await db.Usuarios.FindAsync(id);
                return usuario == null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(new UsuarioResponse(
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Apellido,
                        usuario.Email));
            })
            .WithName("GetUsuarioById")
            .WithSummary("Obtiene un usuario por su ID")
            .Produces<UsuarioResponse>(200)
            .Produces(404)
            .Produces<ErrorResponse>(400);
        }

        // DTOs
        public record CreateUsuarioRequest(
            [Required][StringLength(50)] string Nombre,
            [Required][StringLength(50)] string Apellido,
            [Required][EmailAddress] string Email,
            [Required][StringLength(100, MinimumLength = 6)] string Password);

        public record UsuarioResponse(
            int IdUsuario,
            string Nombre,
            string Apellido,
            string Email);

        public record LoginRequest(
            [Required][EmailAddress] string Email,
            [Required] string Password);

        public record LoginResponse(
            int IdUsuario,
            string Nombre,
            string Apellido,
            string Email);

        public record ErrorResponse(string Message);
    }
}