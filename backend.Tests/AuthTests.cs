using System.Net;
using System.Net.Http.Json;
using backend.DTO;
using backend.security;

namespace backend.Tests;

public class AuthTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public AuthTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [InlineData(AppRoles.Editor)]
    [InlineData(AppRoles.Admin)]
    public async Task Registration_always_assigns_only_Gast(
        string requestedRole)
    {
        // Arrange
        var credentials = TestData.NewUser();

        var body = new
        {
            credentials.Email,
            credentials.Password,
            role = requestedRole,
            roles = new[] { requestedRole }
        };

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/Auth/register",
            body);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<GetUser>();

        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(user.Id));
        Assert.Equal(credentials.Email, user.Email);
        Assert.Equal(AppRoles.Gast, Assert.Single(user.Roles));

        using var write = await _client.PostAsJsonAsync(
            "/Project",
            TestData.Body("/Project"));

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
    }

    [Fact]
    public async Task Login_issues_a_usable_authentication_cookie()
    {
        // Arrange
        var credentials = TestData.NewUser();

        using var registration = await _client.PostAsJsonAsync(
            "/Auth/register",
            credentials);

        registration.EnsureSuccessStatusCode();

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/Auth/login",
            new LoginRequest(
                credentials.Email,
                credentials.Password));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<GetUser>();

        Assert.NotNull(user);
        Assert.Equal(credentials.Email, user.Email);
        Assert.Equal(AppRoles.Gast, Assert.Single(user.Roles));

        Assert.True(
            response.Headers.TryGetValues("Set-Cookie", out var cookies));

        Assert.Contains(
            cookies,
            cookie => cookie.StartsWith("Wpfw.Auth="));

        using var write = await _client.PostAsJsonAsync(
            "/Project",
            TestData.Body("/Project"));

        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Wrong_password_or_unknown_user_returns_401(
        bool existingUser)
    {
        // Arrange
        var credentials = TestData.NewUser();

        using var registration = await _client.PostAsJsonAsync(
            "/Auth/register",
            credentials);

        registration.EnsureSuccessStatusCode();

        var email = existingUser
            ? credentials.Email
            : "unknown@example.test";

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/Auth/login",
            new LoginRequest(
                email,
                $"Wrong1!{Guid.NewGuid():N}"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        Assert.Equal(
            "Ongeldige inloggegevens",
            await response.Content.ReadFromJsonAsync<string>());

        using var write = await _client.PostAsJsonAsync(
            "/Project",
            TestData.Body("/Project"));

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("password")]
    [InlineData("missing")]
    public async Task Invalid_registration_returns_400(string invalidField)
    {
        // Arrange
        var credentials = TestData.NewUser();

        var email = invalidField == "email"
            ? "not-an-email"
            : credentials.Email;

        var password = invalidField == "password"
            ? "weak"
            : credentials.Password;

        if (invalidField == "missing")
            email = null;

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/Auth/register",
            new RegisterRequest(email, password));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<string>();

        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public async Task Logout_removes_the_authenticated_session()
    {
        // Arrange
        await TestData.LoginAsAsync(
            _factory,
            _client,
            AppRoles.Gast);

        // Act
        using var response = await _client.PostAsync(
            "/Auth/logout",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var write = await _client.PostAsJsonAsync(
            "/Project",
            TestData.Body("/Project"));

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}