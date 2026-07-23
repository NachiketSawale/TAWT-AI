using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;
using TawtAi.Api.Services;

namespace TawtAi.Api.Tests;

public class AuthAppServiceTests
{
    private sealed class FakeAzureDevOpsAuthService(bool success, string message, AzureDevOpsProfile? profile) : IAzureDevOpsAuthService
    {
        public string Organization => "ribdev";

        public Task<(bool Success, string Message, AzureDevOpsProfile? Profile)> ValidatePatAsync(
            string patToken, CancellationToken cancellationToken = default) =>
            Task.FromResult((success, message, profile));
    }

    private sealed class FakeJwtTokenService(string token, DateTime expiresAtUtc) : IJwtTokenService
    {
        public (string Token, DateTime ExpiresAtUtc) GenerateToken(AzureDevOpsProfile profile, string organization) =>
            (token, expiresAtUtc);
    }

    [Fact]
    public async Task LoginAsync_WithValidPat_ReturnsSuccessWithTokenAndProfile()
    {
        var profile = new AzureDevOpsProfile { Id = "1", DisplayName = "Jane Doe", EmailAddress = "jane@example.com" };
        var expiresAtUtc = DateTime.UtcNow.AddHours(1);
        var service = new AuthAppService(
            new FakeAzureDevOpsAuthService(true, "Token validated successfully.", profile),
            new FakeJwtTokenService("signed-token", expiresAtUtc));

        var result = await service.LoginAsync(new PatLoginRequest { PatToken = "good-pat" });

        Assert.True(result.Success);
        Assert.Equal("signed-token", result.AccessToken);
        Assert.Equal(expiresAtUtc, result.ExpiresAtUtc);
        Assert.Equal("Jane Doe", result.DisplayName);
        Assert.Equal("jane@example.com", result.EmailAddress);
        Assert.Equal("ribdev", result.Organization);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPat_ReturnsFailureWithoutIssuingToken()
    {
        var service = new AuthAppService(
            new FakeAzureDevOpsAuthService(false, "Invalid Personal Access Token.", null),
            new FakeJwtTokenService("should-not-be-used", DateTime.UtcNow));

        var result = await service.LoginAsync(new PatLoginRequest { PatToken = "bad-pat" });

        Assert.False(result.Success);
        Assert.Equal("Invalid Personal Access Token.", result.Message);
        Assert.Null(result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WhenProfileIsNullDespiteSuccess_ReturnsFailure()
    {
        var service = new AuthAppService(
            new FakeAzureDevOpsAuthService(true, "Unable to read Azure DevOps profile.", null),
            new FakeJwtTokenService("should-not-be-used", DateTime.UtcNow));

        var result = await service.LoginAsync(new PatLoginRequest { PatToken = "pat" });

        Assert.False(result.Success);
    }
}
