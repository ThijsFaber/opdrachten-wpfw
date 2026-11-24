using backend.data;
using backend.DTO;
using backend.security;
using backend.services.common;
using Microsoft.AspNetCore.Identity;

namespace backend.services;

public class AuthService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIns;
    private readonly ApplicationDbContext _db;

    public AuthService(
        UserManager<IdentityUser> users,
        SignInManager<IdentityUser> signIns,
        ApplicationDbContext db)
    {
        _users = users;
        _signIns = signIns;
        _db = db;
    }

    public async Task<ServiceResult<GetUser>> RegisterAsync(
        RegisterRequest dto)
    {
        var error = InputValidation.RequiredFields(
            "Email en wachtwoord zijn verplicht",
            dto.Email,
            dto.Password);

        if (error is not null)
            return ServiceResult<GetUser>.Invalid(error);

        var user = new IdentityUser
        {
            UserName = dto.Email!.Trim(),
            Email = dto.Email!.Trim()
        };

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var created = await _users.CreateAsync(
            user,
            dto.Password!);

        if (!created.Succeeded)
        {
            var errors = string.Join(
                "; ",
                created.Errors.Select(error => error.Description));

            return ServiceResult<GetUser>.Invalid(errors);
        }

        var assigned = await _users.AddToRoleAsync(
            user,
            AppRoles.Gast);

        if (!assigned.Succeeded)
        {
            throw new InvalidOperationException(
                "De Gast-rol kon niet worden toegekend.");
        }

        await transaction.CommitAsync();

        return ServiceResult<GetUser>.Success(
            await ToDtoAsync(user));
    }

    public async Task<ServiceResult<GetUser>> LoginAsync(
        LoginRequest dto)
    {
        var error = InputValidation.RequiredFields(
            "Email en wachtwoord zijn verplicht",
            dto.Email,
            dto.Password);

        if (error is not null)
            return ServiceResult<GetUser>.Invalid(error);

        var user = await _users.FindByEmailAsync(
            dto.Email!.Trim());

        if (user is null)
        {
            return ServiceResult<GetUser>.Unauthorized(
                "Ongeldige inloggegevens");
        }

        var signedIn = await _signIns.PasswordSignInAsync(
            user,
            dto.Password!,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!signedIn.Succeeded)
        {
            return ServiceResult<GetUser>.Unauthorized(
                "Ongeldige inloggegevens");
        }

        return ServiceResult<GetUser>.Success(
            await ToDtoAsync(user));
    }

    public Task LogoutAsync()
    {
        return _signIns.SignOutAsync();
    }

    private async Task<GetUser> ToDtoAsync(IdentityUser user)
    {
        return new GetUser(
            user.Id,
            user.Email!,
            await _users.GetRolesAsync(user));
    }
}