using backend.DTO;
using backend.services;
using backend.services.common;
using backend.security;

namespace backend.endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var projects = endpoints.MapGroup("/Project");

        projects.MapPost("/", async (
            PostProject dto,
            ProjectService service) =>
        {
            var result = await service.CreateAsync(dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            var project = result.Value!;

            return Results.Created(
                $"/Project/{project.Id}",
                project);
        }).RequireAuthorization(AppPolicies.WriteContent);

        projects.MapGet("/{id:int}", async (
            int id,
            ProjectService service) =>
        {
            var project = await service.GetByIdAsync(id);

            if (project is null)
                return Results.NotFound();

            return Results.Ok(project);
        });

        projects.MapGet("/", async (
            ProjectService service) =>
        {
            var projects = await service.GetAllAsync();

            return Results.Ok(projects);
        });

        projects.MapPut("/{id:int}", async (
            int id,
            PutProject dto,
            ProjectService service) =>
        {
            var result = await service.UpdateAsync(id, dto);

            if (result.Status == ServiceResultStatus.Invalid)
                return Results.BadRequest(result.Error);

            if (result.Status == ServiceResultStatus.NotFound)
                return Results.NotFound("Project niet gevonden");

            return Results.Ok(result.Value);
        }).RequireAuthorization(AppPolicies.WriteContent);

        projects.MapDelete("/{id:int}", async (
            int id,
            ProjectService service) =>
        {
            var deleted = await service.DeleteAsync(id);

            if (!deleted)
                return Results.NotFound();

            return Results.NoContent();
        }).RequireAuthorization(AppPolicies.WriteContent);

        return endpoints;
    }
}