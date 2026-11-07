using backend.DTO;
using backend.services;

namespace backend.endpoints;

public static class BlogpostEndpoints
{
    public static IEndpointRouteBuilder MapBlogPostEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var blogposts = endpoints.MapGroup("/BlogPost");

        blogposts.MapPost("/", async (
            PostBlogpost dto,
            BlogpostService service
        ) =>
        {
            var result = await service.CreateAsync(dto);

            if (result.Error is not null)
                return Results.BadRequest(result.Error);

            var blogpost = result.Blogpost!;

            return Results.Created(
                $"/BlogPost/{blogpost.Id}",
                blogpost
            );
        });

        blogposts.MapGet("/{id:int}", async (
            int id,
            BlogpostService service
        ) =>
        {
            var blogpost = await service.GetByIdAsync(id);

            if (blogpost is null)
                return Results.NotFound();

            return Results.Ok(blogpost);
        });

        blogposts.MapGet("/", async (
            BlogpostService service
        ) =>
        {
            var blogposts = await service.GetAllAsync();

            return Results.Ok(blogposts);
        });

        blogposts.MapPut("/{id:int}", async (
            int id,
            PutBlogpost dto,
            BlogpostService service
        ) =>
        {
            var result = await service.UpdateAsync(id, dto);

            if (result.Error is not null)
                return Results.BadRequest(result.Error);

            if (result.Blogpost is null)
                return Results.NotFound("Blogpost niet gevonden");

            return Results.Ok(result.Blogpost);
        });

        blogposts.MapDelete("/{id:int}", async (
            int id,
            BlogpostService service
        ) =>
        {
            var deleted = await service.DeleteAsync(id);

            if (!deleted)
                return Results.NotFound();

            return Results.NoContent();
        });

        return endpoints;
    }
}