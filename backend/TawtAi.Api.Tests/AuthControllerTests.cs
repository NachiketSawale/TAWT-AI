using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;

namespace TawtAi.Api.Tests;

public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private sealed class FakeAzureDevOpsAuthService(bool success, string message, AzureDevOpsProfile? profile) : IAzureDevOpsAuthService
    {
        public string Organization => "ribdev";

        public Task<(bool Success, string Message, AzureDevOpsProfile? Profile)> ValidatePatAsync(
            string patToken, CancellationToken cancellationToken = default) =>
            Task.FromResult((success, message, profile));
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public (string Token, DateTime ExpiresAtUtc) GenerateToken(AzureDevOpsProfile profile, string organization) =>
            ("fake-jwt-token", DateTime.UtcNow.AddHours(1));
    }

    private WebApplicationFactory<Program> WithAzureDevOpsAuthService(IAzureDevOpsAuthService fake) =>
        factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAzureDevOpsAuthService>();
                services.AddScoped(_ => fake);
                services.RemoveAll<IJwtTokenService>();
                services.AddScoped<IJwtTokenService, FakeJwtTokenService>();
            });
        });

    [Fact]
    public async Task Login_WithInvalidPat_ReturnsUnauthorized()
    {
        var client = WithAzureDevOpsAuthService(
            new FakeAzureDevOpsAuthService(false, "Invalid Personal Access Token.", null)).CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new PatLoginRequest { PatToken = "bad-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PatLoginResponse>();
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    [Fact]
    public async Task Login_WithValidPat_ReturnsOkWithAccessToken()
    {
        var profile = new AzureDevOpsProfile { Id = "1", DisplayName = "Jane Doe", EmailAddress = "jane@example.com" };
        var client = WithAzureDevOpsAuthService(
            new FakeAzureDevOpsAuthService(true, "Token validated successfully.", profile)).CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new PatLoginRequest { PatToken = "good-token" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PatLoginResponse>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Equal("fake-jwt-token", body.AccessToken);
    }

    [Fact]
    public async Task Login_WithEmptyPatToken_ReturnsBadRequest()
    {
        var client = WithAzureDevOpsAuthService(
            new FakeAzureDevOpsAuthService(false, "should not be called", null)).CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new PatLoginRequest { PatToken = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
