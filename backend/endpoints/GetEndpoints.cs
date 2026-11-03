using backend.DTO;
using backend.data;
using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.endpoints;

public static class GetEndpoints
{
    public static IEndpointRouteBuilder MapGetEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var get = endpoints.MapGroup("/get");

        get.MapGet("/Blogpost/{id:int}", async (
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

        get.MapGet("/Blogpost", async (
            ApplicationDbContext db
        ) =>
        {
            var blogposts = await db.Blogposts.Select(b => new GetBlogpost(
                b.Id,
                b.Title,
                b.Inhoud,
                b.Publicatiedatum
            )).ToListAsync();

            return Results.Ok(blogposts);
        });

        get.MapGet("/Project/{id:int}", async (
            int id,
            ApplicationDbContext db
        ) =>
        {
            var project = await db.projects.FindAsync(id);

            if (project is null)
                return Results.NotFound();

            var dto = new GetProject(
                project.Id,
                project.Title,
                project.Categorie,
                project.Beschrijving,
                project.GithubUrl,
                project.Datum
            );

            return Results.Ok(dto);
        });

        get.MapGet("/Project", async (
            ApplicationDbContext db
        ) =>
        {
            var projects = await db.projects
                .Select(p => new GetProject(
                    p.Id,
                    p.Title,
                    p.Categorie,
                    p.Beschrijving,
                    p.GithubUrl,
                    p.Datum
                ))
                .ToListAsync();

            return Results.Ok(projects);
        });

        return endpoints;
    }
}