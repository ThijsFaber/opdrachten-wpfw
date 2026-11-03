using backend.DTO;
using backend.data;

namespace backend.endpoints;

public static class PutEndpoints
{
    public static IEndpointRouteBuilder MapPutEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var put = endpoints.MapGroup("/put");

        put.MapPut("/Blogpost/{id:int}", async (
            int id,
            PutBlogpost dto,
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Inhoud))
            {
                return Results.BadRequest(
                    "Title en/of inhoud mogen niet leeg zijn"
                );
            }

            if (dto.Inhoud.Trim().Length < 10)
            {
                return Results.BadRequest(
                    "Inhoud moet minimaal 10 tekens bevatten"
                );
            }

            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost is null)
            {
                return Results.NotFound("Blogpost niet gevonden");
            }

            blogpost.Title = dto.Title.Trim();
            blogpost.Inhoud = dto.Inhoud.Trim();

            await db.SaveChangesAsync();

            return Results.Ok(blogpost);
        });

        put.MapPut("/Project/{id:int}", async (
            int id,
            PutProject dto,
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Categorie) ||
                string.IsNullOrWhiteSpace(dto.Beschrijving) ||
                string.IsNullOrWhiteSpace(dto.GithubUrl))
            {
                return Results.BadRequest(
                    "Geen lege velden toegestaan"
                );
            }

            var project = await db.projects.FindAsync(id);

            if (project is null)
            {
                return Results.NotFound("Project niet gevonden");
            }

            project.Title = dto.Title.Trim();
            project.Categorie = dto.Categorie.Trim();
            project.Beschrijving = dto.Beschrijving.Trim();
            project.GithubUrl = dto.GithubUrl.Trim();

            await db.SaveChangesAsync();

            return Results.Ok(project);
        });

        return endpoints;
    }
}