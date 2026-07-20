namespace TawtAi.Api.Interfaces;

using TawtAi.Api.Models;

public interface IAzureDevOpsAuthService
{
    string Organization { get; }

    Task<(bool Success, string Message, AzureDevOpsProfile? Profile)> ValidatePatAsync(string patToken, CancellationToken cancellationToken = default);
}
