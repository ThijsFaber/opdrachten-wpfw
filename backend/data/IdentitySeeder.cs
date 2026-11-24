using backend.security;
using Microsoft.AspNetCore.Identity;

namespace backend.data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        IServiceProvider services)
    {
        var roles = services
            .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[]
        {
            AppRoles.Gast,
            AppRoles.Editor,
            AppRoles.Admin
        })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                EnsureSucceeded(
                    await roles.CreateAsync(
                        new IdentityRole(role)));
            }
        }
    }

    public static async Task SeedTestUsersAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var users = services
            .GetRequiredService<UserManager<IdentityUser>>();

        await SeedUserAsync(
            users,
            "editor@example.test",
            AppRoles.Editor,
            configuration["SeedUsers:EditorPassword"]);

        await SeedUserAsync(
            users,
            "admin@example.test",
            AppRoles.Admin,
            configuration["SeedUsers:AdminPassword"]);
    }

    private static async Task SeedUserAsync(
        UserManager<IdentityUser> users,
        string email,
        string role,
        string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Stel SeedUsers:{role}Password lokaal in.");
        }

        var user = await users.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            EnsureSucceeded(
                await users.CreateAsync(user, password));
        }

        if (!await users.IsInRoleAsync(user, role))
        {
            EnsureSucceeded(
                await users.AddToRoleAsync(user, role));
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description)));
        }
    }
}