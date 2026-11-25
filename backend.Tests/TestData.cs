using System.Net.Http.Json;
using System.Text.Json;
using backend.data;
using backend.DTO;
using backend.models;
using backend.security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests;

internal static class TestData
{
    public static RegisterRequest NewUser()
    {
        return new RegisterRequest(
            $"test-{Guid.NewGuid():N}@example.test",
            $"Test1!{Guid.NewGuid():N}");
    }

    public static object Body(
        string route,
        string title = "Original title")
    {
        return route switch
        {
            "/Project" => new PostProject(
                title,
                "Project description",
                "School",
                "https://example.test/project"),

            "/BlogPost" => new PostBlogpost(
                title,
                "At least ten characters of content"),

            _ => throw new ArgumentException(
                "Unknown route",
                nameof(route))
        };
    }

    public static async Task LoginAsAsync(
        ApiFactory factory,
        HttpClient client,
        string role)
    {
        var credentials = NewUser();

        using var registration = await client.PostAsJsonAsync(
            "/Auth/register",
            credentials);

        registration.EnsureSuccessStatusCode();

        if (role != AppRoles.Gast)
        {
            using var scope = factory.Services.CreateScope();

            var users = scope.ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            var user = await users.FindByEmailAsync(credentials.Email!);

            Assert.NotNull(user);

            var assigned = await users.AddToRoleAsync(user, role);

            Assert.True(
                assigned.Succeeded,
                string.Join(
                    "; ",
                    assigned.Errors.Select(error => error.Description)));
        }

        using var login = await client.PostAsJsonAsync(
            "/Auth/login",
            new LoginRequest(
                credentials.Email,
                credentials.Password));

        login.EnsureSuccessStatusCode();
    }

    public static async Task<int> SeedContentAsync(
        ApiFactory factory,
        string route)
    {
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        if (route == "/Project")
        {
            var project = new Project
            {
                Title = "Original title",
                Beschrijving = "Project description",
                Categorie = "School",
                GithubUrl = "https://example.test/project",
                Datum = DateTime.UtcNow
            };

            db.projects.Add(project);
            await db.SaveChangesAsync();

            return project.Id;
        }

        var blogpost = new Blogpost
        {
            Title = "Original title",
            Inhoud = "At least ten characters of content",
            Publicatiedatum = DateTime.UtcNow
        };

        db.Blogposts.Add(blogpost);
        await db.SaveChangesAsync();

        return blogpost.Id;
    }

    public static void AssertContent(
        JsonElement actual,
        string route,
        int id,
        string title = "Original title")
    {
        Assert.Equal(id, actual.GetProperty("id").GetInt32());

        var expected = JsonSerializer.SerializeToElement(
            Body(route, title),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        foreach (var property in expected.EnumerateObject())
        {
            Assert.Equal(
                property.Value.GetString(),
                actual.GetProperty(property.Name).GetString());
        }

        var date = route == "/Project"
            ? "datum"
            : "publicatiedatum";

        Assert.True(
            actual.GetProperty(date).TryGetDateTime(out var timestamp));

        Assert.NotEqual(default, timestamp);
    }
}