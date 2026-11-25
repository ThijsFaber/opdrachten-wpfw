using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.security;

namespace backend.Tests;

public class CrudTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public CrudTests()
    {
        _client = _factory.CreateApiClient();
    }

    [Theory]
    [InlineData("/Project")]
    [InlineData("/BlogPost")]
    public async Task Get_all_is_public_and_returns_saved_data(string route)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);

        // Act
        using var response = await _client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content
            .ReadFromJsonAsync<JsonElement[]>();

        Assert.NotNull(items);

        TestData.AssertContent(
            Assert.Single(items),
            route,
            id);
    }

    [Theory]
    [InlineData("/Project")]
    [InlineData("/BlogPost")]
    public async Task Get_by_id_is_public_and_returns_saved_data(string route)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);

        // Act
        using var response = await _client.GetAsync($"{route}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        TestData.AssertContent(item, route, id);
    }

    [Theory]
    [InlineData("/Project", AppRoles.Editor)]
    [InlineData("/Project", AppRoles.Admin)]
    [InlineData("/BlogPost", AppRoles.Editor)]
    [InlineData("/BlogPost", AppRoles.Admin)]
    public async Task Post_creates_and_persists_content(
        string route,
        string role)
    {
        // Arrange
        await TestData.LoginAsAsync(_factory, _client, role);

        // Act
        using var response = await _client.PostAsJsonAsync(
            route,
            TestData.Body(route));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        var id = item.GetProperty("id").GetInt32();

        Assert.True(id > 0);

        Assert.Equal(
            $"{route}/{id}",
            response.Headers.Location?.OriginalString);

        TestData.AssertContent(item, route, id);

        var saved = await _client.GetFromJsonAsync<JsonElement>(
            $"{route}/{id}");

        TestData.AssertContent(saved, route, id);

        var items = await _client
            .GetFromJsonAsync<JsonElement[]>(route);

        Assert.NotNull(items);
        Assert.Single(items);
    }

    [Theory]
    [InlineData("/Project", AppRoles.Editor)]
    [InlineData("/Project", AppRoles.Admin)]
    [InlineData("/BlogPost", AppRoles.Editor)]
    [InlineData("/BlogPost", AppRoles.Admin)]
    public async Task Put_updates_and_persists_content(
        string route,
        string role)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);
        await TestData.LoginAsAsync(_factory, _client, role);

        // Act
        using var response = await _client.PutAsJsonAsync(
            $"{route}/{id}",
            TestData.Body(route, "Updated title"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        TestData.AssertContent(item, route, id, "Updated title");

        var saved = await _client.GetFromJsonAsync<JsonElement>(
            $"{route}/{id}");

        TestData.AssertContent(saved, route, id, "Updated title");
    }

    [Theory]
    [InlineData("/Project", AppRoles.Editor)]
    [InlineData("/Project", AppRoles.Admin)]
    [InlineData("/BlogPost", AppRoles.Editor)]
    [InlineData("/BlogPost", AppRoles.Admin)]
    public async Task Delete_removes_content(
        string route,
        string role)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);
        await TestData.LoginAsAsync(_factory, _client, role);

        // Act
        using var response = await _client.DeleteAsync($"{route}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());

        using var missing = await _client.GetAsync($"{route}/{id}");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var items = await _client
            .GetFromJsonAsync<JsonElement[]>(route);

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Theory]
    [InlineData("/Project", "GET")]
    [InlineData("/Project", "PUT")]
    [InlineData("/Project", "DELETE")]
    [InlineData("/BlogPost", "GET")]
    [InlineData("/BlogPost", "PUT")]
    [InlineData("/BlogPost", "DELETE")]
    public async Task Unknown_id_returns_404(
        string route,
        string method)
    {
        // Arrange
        if (method != "GET")
        {
            await TestData.LoginAsAsync(
                _factory,
                _client,
                AppRoles.Editor);
        }

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            $"{route}/999999");

        if (method == "PUT")
            request.Content = JsonContent.Create(TestData.Body(route));

        // Act
        using var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        if (method == "PUT")
        {
            var expected = route == "/Project"
                ? "Project niet gevonden"
                : "Blogpost niet gevonden";

            Assert.Equal(
                expected,
                await response.Content.ReadFromJsonAsync<string>());
        }
        else
        {
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }
    }

    [Theory]
    [InlineData("/Project", "POST")]
    [InlineData("/Project", "PUT")]
    [InlineData("/BlogPost", "POST")]
    [InlineData("/BlogPost", "PUT")]
    public async Task Invalid_input_returns_400_without_changing_data(
        string route,
        string method)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);

        await TestData.LoginAsAsync(
            _factory,
            _client,
            AppRoles.Editor);

        var url = method == "POST"
            ? route
            : $"{route}/{id}";

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            url)
        {
            Content = JsonContent.Create(TestData.Body(route, "   "))
        };

        // Act
        using var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var expected = route == "/Project"
            ? "Geen lege velden toegestaan"
            : "Title en/of inhoud mogen niet leeg zijn";

        Assert.Equal(
            expected,
            await response.Content.ReadFromJsonAsync<string>());

        var items = await _client
            .GetFromJsonAsync<JsonElement[]>(route);

        Assert.NotNull(items);

        TestData.AssertContent(
            Assert.Single(items),
            route,
            id);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task Short_blog_content_is_rejected_without_changing_data(
        string method)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, "/BlogPost");

        await TestData.LoginAsAsync(
            _factory,
            _client,
            AppRoles.Editor);

        var url = method == "POST"
            ? "/BlogPost"
            : $"/BlogPost/{id}";

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            url)
        {
            Content = JsonContent.Create(new
            {
                title = "Valid title",
                inhoud = "short"
            })
        };

        // Act
        using var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.Equal(
            "Inhoud moet minimaal 10 tekens bevatten",
            await response.Content.ReadFromJsonAsync<string>());

        var items = await _client
            .GetFromJsonAsync<JsonElement[]>("/BlogPost");

        Assert.NotNull(items);

        TestData.AssertContent(
            Assert.Single(items),
            "/BlogPost",
            id);
    }

    [Theory]
    [InlineData("/Project", "POST", null)]
    [InlineData("/Project", "PUT", null)]
    [InlineData("/Project", "DELETE", null)]
    [InlineData("/BlogPost", "POST", null)]
    [InlineData("/BlogPost", "PUT", null)]
    [InlineData("/BlogPost", "DELETE", null)]
    [InlineData("/Project", "POST", AppRoles.Gast)]
    [InlineData("/Project", "PUT", AppRoles.Gast)]
    [InlineData("/Project", "DELETE", AppRoles.Gast)]
    [InlineData("/BlogPost", "POST", AppRoles.Gast)]
    [InlineData("/BlogPost", "PUT", AppRoles.Gast)]
    [InlineData("/BlogPost", "DELETE", AppRoles.Gast)]
    public async Task Unauthorized_writes_do_not_change_data(
        string route,
        string method,
        string? role)
    {
        // Arrange
        var id = await TestData.SeedContentAsync(_factory, route);

        if (role is not null)
            await TestData.LoginAsAsync(_factory, _client, role);

        var expected = role is null
            ? HttpStatusCode.Unauthorized
            : HttpStatusCode.Forbidden;

        var url = method == "POST"
            ? route
            : $"{route}/{id}";

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            url);

        if (method != "DELETE")
        {
            request.Content = JsonContent.Create(
                TestData.Body(route, "Denied title"));
        }

        // Act
        using var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(expected, response.StatusCode);
        Assert.Null(response.Headers.Location);

        var items = await _client
            .GetFromJsonAsync<JsonElement[]>(route);

        Assert.NotNull(items);

        TestData.AssertContent(
            Assert.Single(items),
            route,
            id);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}