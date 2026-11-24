using backend.DTO;
using backend.services;
using backend.services.common;
using backend.security;

namespace backend.endpoints;

public static class BlogpostEndpoints
{
    public static IEndpointRouteBuilder MapBlogPostEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var blogposts = endpoints.MapGroup("/BlogPost");

        blogposts.MapPost("/", async (
            PostBlogpost dto,
            BlogpostService service) =>
        {
            var result = await service.CreateAsync(dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            var blogpost = result.Value!;

            return Results.Created(
                $"/BlogPost/{blogpost.Id}",
                blogpost);
        }).RequireAuthorization(AppPolicies.WriteContent);

        blogposts.MapGet("/{id:int}", async (
            int id,
            BlogpostService service) =>
        {
            var blogpost = await service.GetByIdAsync(id);

            if (blogpost is null)
                return Results.NotFound();

            return Results.Ok(blogpost);
        });

        blogposts.MapGet("/", async (
            BlogpostService service) =>
        {
            var blogposts = await service.GetAllAsync();

            return Results.Ok(blogposts);
        });

        blogposts.MapPut("/{id:int}", async (
            int id,
            PutBlogpost dto,
            BlogpostService service) =>
        {
            var result = await service.UpdateAsync(id, dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            if (result.Status == ServiceResultStatus.NotFound)
                return Results.NotFound("Blogpost niet gevonden");

            return Results.Ok(result.Value);
        }).RequireAuthorization(AppPolicies.WriteContent);

        blogposts.MapDelete("/{id:int}", async (
            int id,
            BlogpostService service) =>
        {
            var deleted = await service.DeleteAsync(id);

            if (!deleted)
                return Results.NotFound();

            return Results.NoContent();
        }).RequireAuthorization(AppPolicies.WriteContent);

        return endpoints;
    }
}