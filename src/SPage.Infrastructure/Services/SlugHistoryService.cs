using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Seo;
using SPage.Infrastructure.Persistence;

namespace SPage.Infrastructure.Services;

/// <summary>
/// Service quản lý lịch sử slug và 301 redirect.
/// Dùng IMemoryCache để không tạo thêm round-trip Redis cho mỗi request.
/// </summary>
public sealed class SlugHistoryService : ISlugHistoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SlugHistoryService> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);
    private const string CachePrefix = "slug_redirect:";
    private const string NullSentinel = "__null__"; // phân biệt "đã tra, không có" vs "chưa tra"

    public SlugHistoryService(
        ApplicationDbContext context,
        IMemoryCache memoryCache,
        ILogger<SlugHistoryService> logger)
    {
        _context = context;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string?> GetRedirectPathAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CachePrefix}{slug}";

        // Cache HIT
        if (_memoryCache.TryGetValue(cacheKey, out string? cached))
        {
            return cached == NullSentinel ? null : cached;
        }

        // Cache MISS — truy vấn DB, lấy bản ghi MỚI NHẤT (có thể slug đổi nhiều lần)
        var history = await _context.SlugHistories
            .AsNoTracking()
            .Where(h => h.OldSlug == slug)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => h.RedirectPath)
            .FirstOrDefaultAsync(cancellationToken);

        var valueToCache = history ?? NullSentinel;
        _memoryCache.Set(cacheKey, valueToCache, CacheTtl);

        _logger.LogDebug("SlugHistory lookup '{Slug}': {Result}", slug,
            history ?? "not found");

        return history;
    }

    public async Task RecordAsync(
        string oldSlug,
        string newSlug,
        string entityType,
        int entityId,
        string redirectPath,
        string? changedBy = null,
        CancellationToken cancellationToken = default)
    {
        // Tránh duplicate: xóa bản ghi cũ có cùng oldSlug nếu tồn tại
        var existing = await _context.SlugHistories
            .Where(h => h.OldSlug == oldSlug && h.EntityType == entityType && h.EntityId == entityId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
            _context.SlugHistories.RemoveRange(existing);

        _context.SlugHistories.Add(new SlugHistory
        {
            OldSlug = oldSlug,
            NewSlug = newSlug,
            EntityType = entityType,
            EntityId = entityId,
            RedirectPath = redirectPath,
            ChangedBy = changedBy,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(oldSlug);

        _logger.LogInformation(
            "SlugHistory recorded: '{OldSlug}' → '{RedirectPath}' ({EntityType} #{EntityId})",
            oldSlug, redirectPath, entityType, entityId);
    }

    public Task InvalidateCacheAsync(string slug)
    {
        _memoryCache.Remove($"{CachePrefix}{slug}");
        return Task.CompletedTask;
    }
}
