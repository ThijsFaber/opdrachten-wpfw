using Microsoft.EntityFrameworkCore;
using backend.models;

namespace backend.data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Blogpost> Blogposts { get; set; }
    public DbSet<Project> projects { get; set; }
}