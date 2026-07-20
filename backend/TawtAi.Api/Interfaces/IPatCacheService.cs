namespace TawtAi.Api.Interfaces;

public interface IPatCacheService
{
    void Store(string userId, string patToken, DateTime expiresAtUtc);

    string? Get(string userId);
}
