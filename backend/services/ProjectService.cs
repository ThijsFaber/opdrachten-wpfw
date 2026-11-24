using backend.data;
using backend.DTO;
using backend.models;
using backend.services.common;
using Microsoft.EntityFrameworkCore;

namespace backend.services;

public class ProjectService
{
    private readonly ApplicationDbContext _db;

    public ProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<GetProject>> CreateAsync(
        PostProject dto)
    {
        var error = Validate(
            dto.Title,
            dto.Categorie,
            dto.Beschrijving,
            dto.GithubUrl);

        if (error is not null)
            return ServiceResult<GetProject>.Invalid(error);

        var project = new Project
        {
            Title = dto.Title!.Trim(),
            Beschrijving = dto.Beschrijving!.Trim(),
            Categorie = dto.Categorie!.Trim(),
            GithubUrl = dto.GithubUrl!.Trim(),
            Datum = DateTime.UtcNow
        };

        _db.projects.Add(project);
        await _db.SaveChangesAsync();

        return ServiceResult<GetProject>.Success(ToDto(project));
    }

    public async Task<GetProject?> GetByIdAsync(int id)
    {
        var project = await _db.projects.FindAsync(id);

        return project is null ? null : ToDto(project);
    }

    public async Task<List<GetProject>> GetAllAsync()
    {
        return await _db.projects
            .Select(project => new GetProject(
                project.Id,
                project.Title,
                project.Beschrijving,
                project.Categorie,
                project.GithubUrl,
                project.Datum))
            .ToListAsync();
    }

    public async Task<ServiceResult<GetProject>> UpdateAsync(
        int id,
        PutProject dto)
    {
        var error = Validate(
            dto.Title,
            dto.Categorie,
            dto.Beschrijving,
            dto.GithubUrl);

        if (error is not null)
            return ServiceResult<GetProject>.Invalid(error);

        var project = await _db.projects.FindAsync(id);

        if (project is null)
            return ServiceResult<GetProject>.NotFound();

        project.Title = dto.Title!.Trim();
        project.Beschrijving = dto.Beschrijving!.Trim();
        project.Categorie = dto.Categorie!.Trim();
        project.GithubUrl = dto.GithubUrl!.Trim();

        await _db.SaveChangesAsync();

        return ServiceResult<GetProject>.Success(ToDto(project));
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
        return InputValidation.RequiredFields(
            "Geen lege velden toegestaan",
            title,
            categorie,
            beschrijving,
            githubUrl);
    }

    private static GetProject ToDto(Project project)
    {
        return new GetProject(
            project.Id,
            project.Title,
            project.Beschrijving,
            project.Categorie,
            project.GithubUrl,
            project.Datum);
    }
}