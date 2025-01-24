using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using MonetixFogachoReveloAPI.Data;
using MonetixFogachoReveloAPI.Data.Models;
using MonetixFogachoReveloAPI.Interfaz;

namespace MonetixFogachoReveloAPI.Controllers
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/usuarios")
                .WithTags("Usuarios")
                .WithOpenApi();

            group.MapPost("/register", async Task<Results<Created<UsuarioResponse>, BadRequest<ErrorResponse>>> (
                [FromBody] CreateUsuarioRequest request,
                [FromServices] FogachoReveloDataContext db) =>
            {
                // Validación de modelo
                if (!Validator.TryValidateObject(request, new ValidationContext(request), null, true))
                    return TypedResults.BadRequest(new ErrorResponse("Datos de entrada inválidos"));

                // Verificar email único
                if (await db.Usuarios.AnyAsync(u => u.Email == request.Email))
                    return TypedResults.BadRequest(new ErrorResponse("El email ya está registrado"));

                // Mapear a entidad
                var usuario = new Usuario
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    Password = request.Password, // Deberías hashear la contraseña aquí
                    Gastos = new List<Gasto>()
                };

                // Guardar en BD
                await db.Usuarios.AddAsync(usuario);
                await db.SaveChangesAsync();

                // Retornar respuesta sin información sensible
                return TypedResults.Created(
                    $"/api/usuarios/{usuario.IdUsuario}",
                    new UsuarioResponse(
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Apellido,
                        usuario.Email));
            })
            .WithSummary("Registrar nuevo usuario")
            .Produces<UsuarioResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

            group.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<ErrorResponse>, UnauthorizedHttpResult>> (
                [FromBody] LoginRequest request,
                [FromServices] FogachoReveloDataContext db,
                [FromServices] ITokenService tokenService) =>
            {
                // Validación básica
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return TypedResults.BadRequest(new ErrorResponse("Email y contraseña son requeridos"));

                // Buscar usuario
                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                // Validar credenciales
                if (usuario == null || usuario.Password != request.Password) // Deberías comparar hashes aquí
                    return TypedResults.Unauthorized();

                // Generar token JWT
                var token = tokenService.GenerateJwtToken(usuario);

                return TypedResults.Ok(new LoginResponse(
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    token));
            })
            .WithSummary("Iniciar sesión de usuario")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id}", async Task<Results<Ok<UsuarioResponse>, NotFound>> (
                [FromRoute] int id,
                [FromServices] FogachoReveloDataContext db) =>
            {
                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                return usuario is null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(new UsuarioResponse(
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Apellido,
                        usuario.Email));
            })
            .RequireAuthorization()
            .WithSummary("Obtener usuario por ID")
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id}", async Task<Results<NoContent, NotFound, BadRequest<ErrorResponse>>> (
                [FromRoute] int id,
                [FromBody] UpdateUsuarioRequest request,
                [FromServices] FogachoReveloDataContext db) =>
            {
                var usuario = await db.Usuarios.FindAsync(id);
                if (usuario is null) return TypedResults.NotFound();

                // Actualizar campos permitidos
                usuario.Nombre = request.Nombre ?? usuario.Nombre;
                usuario.Apellido = request.Apellido ?? usuario.Apellido;

                // Validar modelo
                if (!Validator.TryValidateObject(usuario, new ValidationContext(usuario), null, true))
                    return TypedResults.BadRequest(new ErrorResponse("Datos inválidos"));

                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            })
            .RequireAuthorization()
            .WithSummary("Actualizar usuario")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }

    // DTOs
    public record CreateUsuarioRequest(
        [Required][StringLength(50)] string Nombre,
        [Required][StringLength(50)] string Apellido,
        [Required][EmailAddress] string Email,
        [Required][StringLength(100, MinimumLength = 6)] string Password);

    public record UpdateUsuarioRequest(
        [StringLength(50)] string? Nombre,
        [StringLength(50)] string? Apellido);

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
        string Email,
        string Token);

    public record ErrorResponse(string Message);
}