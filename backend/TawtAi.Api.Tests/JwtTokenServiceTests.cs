using Microsoft.Extensions.Options;
using TawtAi.Api.Models;
using TawtAi.Api.Services;
using TawtAi.Api.Settings;

namespace TawtAi.Api.Tests;

public class JwtTokenServiceTests
{
    private static readonly AzureDevOpsProfile Profile = new()
    {
        Id = "user-1",
        DisplayName = "Jane Doe",
        EmailAddress = "jane.doe@example.com"
    };

    private static JwtTokenService CreateService(string key = "a-sufficiently-long-test-signing-key-1234567890") =>
        new(Options.Create(new JwtSettings
        {
            Key = key,
            Issuer = "TawtAi.Api.Tests",
            Audience = "TawtAi.Api.Tests.Client",
            ExpiryMinutes = 30
        }));

    [Fact]
    public void GenerateToken_WithValidKey_ReturnsNonEmptyTokenAndFutureExpiry()
    {
        var service = CreateService();

        var (token, expiresAtUtc) = service.GenerateToken(Profile, "ribdev");

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_WithMissingKey_ThrowsInvalidOperationException()
    {
        var service = CreateService(key: "");

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(Profile, "ribdev"));
    }

    [Fact]
    public void GenerateToken_RespectsConfiguredExpiryMinutes()
    {
        var service = CreateService();
        var before = DateTime.UtcNow;

        var (_, expiresAtUtc) = service.GenerateToken(Profile, "ribdev");

        var expectedMax = before.AddMinutes(30).AddSeconds(5);
        Assert.True(expiresAtUtc <= expectedMax);
        Assert.True(expiresAtUtc > before.AddMinutes(29));
    }
}
