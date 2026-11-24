using backend.data;
using backend.DTO;
using backend.models;
using backend.services.common;
using Microsoft.EntityFrameworkCore;

namespace backend.services;

public class BlogpostService
{
    private readonly ApplicationDbContext _db;

    public BlogpostService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<GetBlogpost>> CreateAsync(
        PostBlogpost dto)
    {
        var error = Validate(dto.Title, dto.Inhoud);

        if (error is not null)
            return ServiceResult<GetBlogpost>.Invalid(error);

        var blogpost = new Blogpost
        {
            Title = dto.Title!.Trim(),
            Inhoud = dto.Inhoud!.Trim(),
            Publicatiedatum = DateTime.UtcNow
        };

        _db.Blogposts.Add(blogpost);
        await _db.SaveChangesAsync();

        return ServiceResult<GetBlogpost>.Success(ToDto(blogpost));
    }

    public async Task<GetBlogpost?> GetByIdAsync(int id)
    {
        var blogpost = await _db.Blogposts.FindAsync(id);

        return blogpost is null ? null : ToDto(blogpost);
    }

    public async Task<List<GetBlogpost>> GetAllAsync()
    {
        return await _db.Blogposts
            .Select(blogpost => new GetBlogpost(
                blogpost.Id,
                blogpost.Title,
                blogpost.Inhoud,
                blogpost.Publicatiedatum))
            .ToListAsync();
    }

    public async Task<ServiceResult<GetBlogpost>> UpdateAsync(
        int id,
        PutBlogpost dto)
    {
        var error = Validate(dto.Title, dto.Inhoud);

        if (error is not null)
            return ServiceResult<GetBlogpost>.Invalid(error);

        var blogpost = await _db.Blogposts.FindAsync(id);

        if (blogpost is null)
            return ServiceResult<GetBlogpost>.NotFound();

        blogpost.Title = dto.Title!.Trim();
        blogpost.Inhoud = dto.Inhoud!.Trim();

        await _db.SaveChangesAsync();

        return ServiceResult<GetBlogpost>.Success(ToDto(blogpost));
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
        var error = InputValidation.RequiredFields(
            "Title en/of inhoud mogen niet leeg zijn",
            title,
            inhoud);

        if (error is not null)
            return error;

        if (inhoud!.Trim().Length < 10)
            return "Inhoud moet minimaal 10 tekens bevatten";

        return null;
    }

    private static GetBlogpost ToDto(Blogpost blogpost)
    {
        return new GetBlogpost(
            blogpost.Id,
            blogpost.Title,
            blogpost.Inhoud,
            blogpost.Publicatiedatum);
    }
}