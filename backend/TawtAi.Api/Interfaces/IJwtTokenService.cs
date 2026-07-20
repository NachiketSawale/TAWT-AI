namespace TawtAi.Api.Interfaces;

using TawtAi.Api.Models;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AzureDevOpsProfile profile, string organization);
}
