namespace SPage.Application.Common.Behaviours;

/// <summary>Đánh dấu query sẽ được tự động cache bởi CachingBehaviour pipeline</summary>
public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}
