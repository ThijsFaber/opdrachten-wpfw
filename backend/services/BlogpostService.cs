using backend.DTO;
using backend.data;
using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.services;

public class BlogpostService
{
    private readonly ApplicationDbContext _db;

    public BlogpostService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(GetBlogpost? Blogpost, string? Error)> CreateAsync(
        PostBlogpost dto)
    {
        var error = Validate(dto.Title, dto.Inhoud);

        if (error is not null)
            return (null, error);

        var blogpost = new Blogpost
        {
            Title = dto.Title!,
            Inhoud = dto.Inhoud!,
            Publicatiedatum = DateTime.UtcNow
        };

        _db.Blogposts.Add(blogpost);
        await _db.SaveChangesAsync();

        return (ToDto(blogpost), null);
    }

    public async Task<GetBlogpost?> GetByIdAsync(int id)
    {
        var blogpost = await _db.Blogposts.FindAsync(id);

        return blogpost is null ? null : ToDto(blogpost);
    }

    public async Task<List<GetBlogpost>> GetAllAsync()
    {
        return await _db.Blogposts
            .Select(b => new GetBlogpost(
                b.Id,
                b.Title,
                b.Inhoud,
                b.Publicatiedatum
            ))
            .ToListAsync();
    }

    public async Task<(GetBlogpost? Blogpost, string? Error)> UpdateAsync(
        int id,
        PutBlogpost dto)
    {
        var error = Validate(dto.Title, dto.Inhoud);

        if (error is not null)
            return (null, error);

        var blogpost = await _db.Blogposts.FindAsync(id);

        if (blogpost is null)
            return (null, null);

        blogpost.Title = dto.Title!.Trim();
        blogpost.Inhoud = dto.Inhoud!.Trim();

        await _db.SaveChangesAsync();

        return (ToDto(blogpost), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var blogpost = await _db.Blogposts.FindAsync(id);

        if (blogpost is null)
            return false;

        _db.Blogposts.Remove(blogpost);
        await _db.SaveChangesAsync();

        return true;
    }

    private static string? Validate(string? title, string? inhoud)
    {
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(inhoud))
        {
            return "Title en/of inhoud mogen niet leeg zijn";
        }

        if (inhoud.Trim().Length < 10)
        {
            return "Inhoud moet minimaal 10 tekens bevatten";
        }

        return null;
    }

    private static GetBlogpost ToDto(Blogpost blogpost)
    {
        return new GetBlogpost(
            blogpost.Id,
            blogpost.Title,
            blogpost.Inhoud,
            blogpost.Publicatiedatum
        );
    }
}