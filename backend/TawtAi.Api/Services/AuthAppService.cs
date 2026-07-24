using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;

namespace TawtAi.Api.Services;

public class AuthAppService(IAzureDevOpsAuthService azureDevOpsAuthService, IJwtTokenService jwtTokenService)
{
    public async Task<PatLoginResponse> LoginAsync(PatLoginRequest request, CancellationToken cancellationToken = default)
    {
        var (success, message, profile) = await azureDevOpsAuthService.ValidatePatAsync(request.PatToken, cancellationToken);

        if (!success || profile is null)
        {
            return new PatLoginResponse
            {
                Success = false,
                Message = message
            };
        }

        var organization = azureDevOpsAuthService.Organization;
        var (token, expiresAtUtc) = jwtTokenService.GenerateToken(profile, organization);

        return new PatLoginResponse
        {
            Success = true,
            Message = "Login successful.",
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            DisplayName = profile.DisplayName,
            EmailAddress = profile.EmailAddress,
            Organization = organization
        };
    }
}
