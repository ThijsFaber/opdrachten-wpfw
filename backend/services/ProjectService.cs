using backend.DTO;
using backend.data;
using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.services;

public class ProjectService
{
    private readonly ApplicationDbContext _db;

    public ProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(GetProject? Project, string? Error)> CreateAsync(
        PostProject dto)
    {
        var error = Validate(
            dto.Title,
            dto.Categorie,
            dto.Beschrijving,
            dto.GithubUrl
        );

        if (error is not null)
            return (null, error);

        var project = new Project
        {
            Title = dto.Title!,
            Beschrijving = dto.Beschrijving!,
            Categorie = dto.Categorie!,
            GithubUrl = dto.GithubUrl!,
            Datum = DateTime.UtcNow
        };

        _db.projects.Add(project);
        await _db.SaveChangesAsync();

        return (ToDto(project), null);
    }

    public async Task<GetProject?> GetByIdAsync(int id)
    {
        var project = await _db.projects.FindAsync(id);

        return project is null ? null : ToDto(project);
    }

    public async Task<List<GetProject>> GetAllAsync()
    {
        return await _db.projects
            .Select(p => new GetProject(
                p.Id,
                p.Title,
                p.Beschrijving,
                p.Categorie,
                p.GithubUrl,
                p.Datum
            ))
            .ToListAsync();
    }

    public async Task<(GetProject? Project, string? Error)> UpdateAsync(
        int id,
        PutProject dto)
    {
        var error = Validate(
            dto.Title,
            dto.Categorie,
            dto.Beschrijving,
            dto.GithubUrl
        );

        if (error is not null)
            return (null, error);

        var project = await _db.projects.FindAsync(id);

        if (project is null)
            return (null, null);

        project.Title = dto.Title!.Trim();
        project.Categorie = dto.Categorie!.Trim();
        project.Beschrijving = dto.Beschrijving!.Trim();
        project.GithubUrl = dto.GithubUrl!.Trim();

        await _db.SaveChangesAsync();

        return (ToDto(project), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _db.projects.FindAsync(id);

        if (project is null)
            return false;

        _db.projects.Remove(project);
        await _db.SaveChangesAsync();

        return true;
    }

    private static string? Validate(
        string? title,
        string? categorie,
        string? beschrijving,
        string? githubUrl)
    {
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(categorie) ||
            string.IsNullOrWhiteSpace(beschrijving) ||
            string.IsNullOrWhiteSpace(githubUrl))
        {
            return "Geen lege velden toegestaan";
        }

        return null;
    }

    private static GetProject ToDto(Project project)
    {
        return new GetProject(
            project.Id,
            project.Title,
            project.Beschrijving,
            project.Categorie,
            project.GithubUrl,
            project.Datum
        );
    }
}