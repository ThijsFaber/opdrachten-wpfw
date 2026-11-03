using backend.DTO;
using backend.data;
using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.endpoints;

public static class BlogpostEndpoints
{
    public static IEndpointRouteBuilder MapBlogPostEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var blogposts = endpoints.MapGroup("/BlogPost");

        blogposts.MapPost("/", async (
            PostBlogpost dto,
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Inhoud))
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

            var blogpost = new Blogpost
            {
                Title = dto.Title,
                Inhoud = dto.Inhoud,
                Publicatiedatum = DateTime.UtcNow
            };

            db.Blogposts.Add(blogpost);
            await db.SaveChangesAsync();

            var response = new GetBlogpost(
                blogpost.Id,
                blogpost.Title,
                blogpost.Inhoud,
                blogpost.Publicatiedatum
            );

            return Results.Created($"/Blogpost/{blogpost.Id}", response);
        });

        blogposts.MapGet("/{id:int}", async (
            int id,
            ApplicationDbContext db
        ) =>
        {
            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost is null)
                return Results.NotFound();

            var dto = new GetBlogpost(
              blogpost.Id,
              blogpost.Title,
              blogpost.Inhoud,
              blogpost.Publicatiedatum
            );

            return Results.Ok(dto);
        });

        blogposts.MapGet("/", async (
            ApplicationDbContext db
        ) =>
        {
            var Allblogposts = await db.Blogposts.Select(b => new GetBlogpost(
                b.Id,
                b.Title,
                b.Inhoud,
                b.Publicatiedatum
            )).ToListAsync();

            return Results.Ok(Allblogposts);
        });

        blogposts.MapPut("/{id:int}", async (
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

            var response = new GetBlogpost(
                blogpost.Id,
                blogpost.Title,
                blogpost.Inhoud,
                blogpost.Publicatiedatum
            );

            return Results.Ok(response);
        });

        blogposts.MapDelete("/{id:int}", async (
            int id,
            ApplicationDbContext db
        ) =>
        {
            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost is null)
                return Results.NotFound();

            db.Blogposts.Remove(blogpost);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return endpoints;
    }
}