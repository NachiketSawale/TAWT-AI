using Microsoft.Extensions.Caching.Memory;
using TawtAi.Api.Interfaces;

namespace TawtAi.Api.Services;

public class PatCacheService : IPatCacheService
{
    private const string KeyPrefix = "pat:";
    private readonly IMemoryCache _cache;

    public PatCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Store(string userId, string patToken, DateTime expiresAtUtc)
    {
        _cache.Set(KeyPrefix + userId, patToken, expiresAtUtc);
    }

    public string? Get(string userId)
    {
        return _cache.TryGetValue(KeyPrefix + userId, out string? patToken) ? patToken : null;
    }
}
