namespace TawtAi.Api.Models;

public class PatLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string? DisplayName { get; set; }
    public string? EmailAddress { get; set; }
    public string? Organization { get; set; }
}
