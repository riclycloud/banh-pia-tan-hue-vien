using MediatR;
using Microsoft.Extensions.Logging;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Common.Behaviours;

public sealed class CachingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(ICacheService cache, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache HIT: {CacheKey}", request.CacheKey);
            return cached;
        }

        var result = await next();

        await _cache.SetAsync(
            request.CacheKey,
            result,
            request.CacheDuration ?? TimeSpan.FromMinutes(10),
            cancellationToken);

        _logger.LogDebug("Cache SET: {CacheKey}", request.CacheKey);
        return result;
    }
}
