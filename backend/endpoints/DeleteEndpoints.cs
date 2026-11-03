using backend.data;

namespace backend.endpoints;

public static class DeleteEndpoints
{
    public static IEndpointRouteBuilder MapDeleteEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var delete = endpoints.MapGroup("/delete");

        delete.MapDelete("/Blogpost/{id:int}", async (
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

        delete.MapDelete("/Project/{id:int}", async (
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