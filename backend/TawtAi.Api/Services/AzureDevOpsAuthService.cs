using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;
using TawtAi.Api.Settings;

namespace TawtAi.Api.Services;

public class AzureDevOpsAuthService : IAzureDevOpsAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AzureDevOpsSettings _settings;
    private readonly ILogger<AzureDevOpsAuthService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string Organization => _settings.Organization;

    public AzureDevOpsAuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureDevOpsSettings> settings,
        ILogger<AzureDevOpsAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, AzureDevOpsProfile? Profile)> ValidatePatAsync(
        string patToken, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(nameof(AzureDevOpsAuthService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{patToken}")));

        try
        {
            var profileUrl = $"{_settings.IdentityBaseUrl.TrimEnd('/')}/{_settings.Organization}/_apis/profile/profiles/me?api-version={_settings.ApiVersion}";
            var profileResponse = await client.GetAsync(profileUrl, cancellationToken);
            if (!profileResponse.IsSuccessStatusCode)
            {
                var body = await profileResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("PAT profile check failed. Status: {StatusCode} {ReasonPhrase}. Body: {Body}",
                    (int)profileResponse.StatusCode, profileResponse.ReasonPhrase, body);
                return (false, "Invalid Personal Access Token.", null);
            }

            var profileJson = await profileResponse.Content.ReadAsStringAsync(cancellationToken);
            var profile = JsonSerializer.Deserialize<AzureDevOpsProfile>(profileJson, JsonOptions);

            if (profile is null)
            {
                return (false, "Unable to read Azure DevOps profile.", null);
            }

            var orgCheckUrl = $"{_settings.OrganizationBaseUrl.TrimEnd('/')}/{_settings.Organization}/_apis/projects?api-version={_settings.ApiVersion}";
            var orgResponse = await client.GetAsync(orgCheckUrl, cancellationToken);
            if (!orgResponse.IsSuccessStatusCode)
            {
                var body = await orgResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "PAT organization check failed for '{Organization}'. Status: {StatusCode} {ReasonPhrase}. Body: {Body}",
                    _settings.Organization, (int)orgResponse.StatusCode, orgResponse.ReasonPhrase, body);
                return (false, $"Token is valid but does not have access to organization '{_settings.Organization}'.", null);
            }

            return (true, "Token validated successfully.", profile);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PAT validation against Azure DevOps failed.");
            return (false, "Unable to validate token against Azure DevOps.", null);
        }
    }
}
