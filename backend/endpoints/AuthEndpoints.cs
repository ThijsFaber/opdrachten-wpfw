using backend.DTO;
using backend.services;
using backend.services.common;

namespace backend.endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/Auth");

        auth.MapPost("/register", async (
            RegisterRequest dto,
            AuthService service) =>
        {
            var result = await service.RegisterAsync(dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            return Results.Json(
                result.Value,
                statusCode: StatusCodes.Status201Created);
        }).AllowAnonymous();

        auth.MapPost("/login", async (
            LoginRequest dto,
            AuthService service) =>
        {
            var result = await service.LoginAsync(dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            if (result.Status == ServiceResultStatus.Unauthorized)
            {
                return Results.Json(
                    result.Error,
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(result.Value);
        }).AllowAnonymous();

        auth.MapPost("/logout", async (
            AuthService service) =>
        {
            await service.LogoutAsync();

            return Results.NoContent();
        }).RequireAuthorization();

        return endpoints;
    }
}