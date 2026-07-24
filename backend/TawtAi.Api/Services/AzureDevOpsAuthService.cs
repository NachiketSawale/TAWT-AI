using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TawtAi.Api.Interfaces;
using TawtAi.Api.Models;
using TawtAi.Api.Settings;

namespace TawtAi.Api.Services;

public class AzureDevOpsAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<AzureDevOpsSettings> settings,
    ILogger<AzureDevOpsAuthService> logger) : IAzureDevOpsAuthService
{
    private readonly AzureDevOpsSettings _settings = settings.Value;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string Organization => _settings.Organization;

    public async Task<(bool Success, string Message, AzureDevOpsProfile? Profile)> ValidatePatAsync(
        string patToken, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(nameof(AzureDevOpsAuthService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($":{patToken}")));

        try
        {
            var profileUrl = $"{_settings.IdentityBaseUrl.TrimEnd('/')}/{_settings.Organization}/_apis/profile/profiles/me?api-version={_settings.ApiVersion}";
            var orgCheckUrl = $"{_settings.OrganizationBaseUrl.TrimEnd('/')}/{_settings.Organization}/_apis/projects?api-version={_settings.ApiVersion}";

            var profileTask = client.GetAsync(profileUrl, cancellationToken);
            var orgTask = client.GetAsync(orgCheckUrl, cancellationToken);
            await Task.WhenAll(profileTask, orgTask);

            var profileResponse = await profileTask;
            var orgResponse = await orgTask;

            if (!profileResponse.IsSuccessStatusCode)
            {
                var body = await profileResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("PAT profile check failed. Status: {StatusCode} {ReasonPhrase}. Body: {Body}",
                    (int)profileResponse.StatusCode, profileResponse.ReasonPhrase, body);
                return (false, "Invalid Personal Access Token.", null);
            }

            if (!orgResponse.IsSuccessStatusCode)
            {
                var body = await orgResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "PAT organization check failed for '{Organization}'. Status: {StatusCode} {ReasonPhrase}. Body: {Body}",
                    _settings.Organization, (int)orgResponse.StatusCode, orgResponse.ReasonPhrase, body);
                return (false, $"Token is valid but does not have access to organization '{_settings.Organization}'.", null);
            }

            var profileJson = await profileResponse.Content.ReadAsStringAsync(cancellationToken);

            AzureDevOpsProfile? profile;
            try
            {
                profile = JsonSerializer.Deserialize<AzureDevOpsProfile>(profileJson, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex,
                    "PAT profile check returned a non-JSON response (likely an auth redirect to an HTML page). Body: {Body}",
                    profileJson);
                return (false, "Invalid Personal Access Token.", null);
            }

            if (profile is null)
            {
                return (false, "Unable to read Azure DevOps profile.", null);
            }

            return (true, "Token validated successfully.", profile);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "PAT validation against Azure DevOps failed.");
            return (false, "Unable to validate token against Azure DevOps.", null);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }
}
