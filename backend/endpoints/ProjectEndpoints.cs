using backend.DTO;
using backend.data;
using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var projects = endpoints.MapGroup("/Project");

        projects.MapPost("/", async (
            PostProject dto,
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Categorie) || string.IsNullOrWhiteSpace(dto.Beschrijving) || string.IsNullOrWhiteSpace(dto.GithubUrl))
            {
                return Results.BadRequest(
                    "Geen lege velden toegestaan"
                );
            }

            var project = new Project
            {
                Title = dto.Title,
                Beschrijving = dto.Beschrijving,
                Categorie = dto.Categorie,
                GithubUrl = dto.GithubUrl,
                Datum = DateTime.UtcNow
            };

            db.projects.Add(project);
            await db.SaveChangesAsync();

            var response = new GetProject
            (
                project.Id,
                project.Title,
                project.Beschrijving,
                project.Categorie,
                project.GithubUrl,
                project.Datum
            );

            return Results.Created($"/Project/{project.Id}", response);
        }
        );

        projects.MapGet("/{id:int}", async (
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
                project.Beschrijving,
                project.Categorie,
                project.GithubUrl,
                project.Datum
            );

            return Results.Ok(dto);
        });

        projects.MapGet("/", async (
            ApplicationDbContext db
        ) =>
        {
            var dto = await db.projects
                .Select(p => new GetProject(
                    p.Id,
                    p.Title,
                    p.Beschrijving,
                    p.Categorie,
                    p.GithubUrl,
                    p.Datum
                ))
                .ToListAsync();

            return Results.Ok(dto);
        });

        projects.MapPut("/{id:int}", async (
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

            var response = new GetProject(
                project.Id,
                project.Title,
                project.Beschrijving,
                project.Categorie,
                project.GithubUrl,
                project.Datum
            );

            return Results.Ok(response);
        });

        projects.MapDelete("/{id:int}", async (
            int id,
            ApplicationDbContext db
        ) =>
        {
            var project = await db.projects.FindAsync(id);

            if (project is null)
                return Results.NotFound();

            db.projects.Remove(project);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
        return endpoints;
    }
}