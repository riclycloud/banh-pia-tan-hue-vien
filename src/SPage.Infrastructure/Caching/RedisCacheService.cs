using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using SPage.Application.Common.Interfaces;

namespace SPage.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public RedisCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        }, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _cache.RemoveAsync(key, cancellationToken);

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Cần StackExchange.Redis IConnectionMultiplexer để xóa theo pattern
        // TODO: inject IConnectionMultiplexer nếu cần xóa hàng loạt
        return Task.CompletedTask;
    }
}
