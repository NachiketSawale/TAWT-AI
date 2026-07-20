using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;

namespace TawtAi.Api.Services;

public class AuthAppService
{
    private readonly IAzureDevOpsAuthService _azureDevOpsAuthService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPatCacheService _patCacheService;

    public AuthAppService(
        IAzureDevOpsAuthService azureDevOpsAuthService,
        IJwtTokenService jwtTokenService,
        IPatCacheService patCacheService)
    {
        _azureDevOpsAuthService = azureDevOpsAuthService;
        _jwtTokenService = jwtTokenService;
        _patCacheService = patCacheService;
    }

    public async Task<PatLoginResponse> LoginAsync(PatLoginRequest request, CancellationToken cancellationToken = default)
    {
        var (success, message, profile) = await _azureDevOpsAuthService.ValidatePatAsync(request.PatToken, cancellationToken);

        if (!success || profile is null)
        {
            return new PatLoginResponse
            {
                Success = false,
                Message = message
            };
        }

        var organization = _azureDevOpsAuthService.Organization;
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(profile, organization);

        if (!string.IsNullOrEmpty(profile.Id))
        {
            _patCacheService.Store(profile.Id, request.PatToken, expiresAtUtc);
        }

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
