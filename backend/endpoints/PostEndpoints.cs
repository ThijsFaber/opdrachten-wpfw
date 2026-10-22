using backend.DTO;
using backend.data;
using backend.models;

namespace backend.endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var post = endpoints.MapGroup("/post");

        post.MapPost("/Blogpost", async (
            PostBlogpost dto, 
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Inhoud))
            {
                return Results.BadRequest(
                    "Title en/of inhoud mogen niet leeg zijn"
                );
            };

            if (dto.Inhoud.Trim().Length < 10)
            {
                return Results.BadRequest(
                    "Inhoud moet langer dan 10 tekens zijn"
                );
            }

            var blogpost = new Blogpost
            {
              Title = dto.Title,
              Inhoud = dto.Inhoud,
              Publicatiedatum = DateTime.UtcNow  
            };

            db.Blogposts.Add(blogpost);
            await db.SaveChangesAsync();

            return Results.Created($"/blogpost/{blogpost.Id}", blogpost);
        });

        return endpoints;
    }
}