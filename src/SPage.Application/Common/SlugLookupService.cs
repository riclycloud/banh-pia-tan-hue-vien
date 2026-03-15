using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Seo;

namespace SPage.Application.Common;

/// <summary>
/// Đồng bộ bảng SlugLookups: một nơi duy nhất xác định RewritePath/RewriteQuery theo loại entity.
/// </summary>
public sealed class SlugLookupService : ISlugLookupService
{
    private const string PathPostsDetail = "/Posts/Detail";
    private const string PathPostsIndex = "/Posts/Index";
    private const string PathProductsIndex = "/Products/Index";
    private const string PathProductsDetail = "/Products/Detail";

    private readonly IApplicationDbContext _context;

    public SlugLookupService(IApplicationDbContext context) => _context = context;

    /// <summary>Trả về true nếu slug đã được dùng bởi entity khác cùng loại (trùng Slug + EntityType, khác EntityId).</summary>
    public async Task<bool> IsSlugInUseAsync(
        string slug,
        string? excludeEntityType = null,
        int? excludeEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(slug);
        if (string.IsNullOrEmpty(normalized)) return false;

        var query = _context.SlugLookups.Where(x => x.Slug == normalized);
        if (!string.IsNullOrEmpty(excludeEntityType))
            query = query.Where(x => x.EntityType == excludeEntityType);
        if (excludeEntityId.HasValue)
            query = query.Where(x => x.EntityId != excludeEntityId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public Task SyncPostAsync(string? slug, int postId, CancellationToken cancellationToken = default)
    {
        var path = PathPostsDetail;
        var s = slug ?? string.Empty;
        var query = $"slug={Uri.EscapeDataString(s)}";
        return UpsertAsync(s, SlugLookupConstants.TypePost, postId, path, query, cancellationToken);
    }

    public Task SyncCategoryAsync(string? slug, int categoryId, CancellationToken cancellationToken = default)
    {
        var path = PathPostsIndex;
        var query = $"category={categoryId}";
        return UpsertAsync(slug ?? string.Empty, SlugLookupConstants.TypeCategory, categoryId, path, query, cancellationToken);
    }

    public Task SyncProductCategoryAsync(string? slug, int categoryId, CancellationToken cancellationToken = default)
    {
        var path = PathProductsIndex;
        var query = $"category={categoryId}";
        return UpsertAsync(slug ?? string.Empty, SlugLookupConstants.TypeProductCategory, categoryId, path, query, cancellationToken);
    }

    public Task SyncProductAsync(string? slug, int productId, string categorySlug, CancellationToken cancellationToken = default)
    {
        var path = PathProductsDetail;
        var s = slug ?? string.Empty;
        var cat = string.IsNullOrWhiteSpace(categorySlug) ? "san-pham" : categorySlug.Trim();
        var query = $"categorySlug={Uri.EscapeDataString(cat)}&productSlug={Uri.EscapeDataString(s)}";
        return UpsertAsync(s, SlugLookupConstants.TypeProduct, productId, path, query, cancellationToken);
    }

    public async Task RemoveAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var row = await _context.SlugLookups
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.EntityId == entityId, cancellationToken);
        if (row is not null)
            _context.SlugLookups.Remove(row);
    }

    private async Task UpsertAsync(
        string slug,
        string entityType,
        int entityId,
        string rewritePath,
        string rewriteQuery,
        CancellationToken cancellationToken)
    {
        var normalized = Normalize(slug);
        if (string.IsNullOrEmpty(normalized)) return;

        var existing = await _context.SlugLookups
            .FirstOrDefaultAsync(x => x.EntityType == entityType && x.EntityId == entityId, cancellationToken);

        if (existing is not null)
        {
            if (!existing.Slug.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                if (await IsSlugInUseAsync(normalized, entityType, entityId, cancellationToken))
                    throw new InvalidOperationException($"Slug '{normalized}' đã được sử dụng.");
            }
            existing.Slug = normalized;
            existing.RewritePath = rewritePath;
            existing.RewriteQuery = rewriteQuery;
        }
        else
        {
            if (await IsSlugInUseAsync(normalized, entityType, null, cancellationToken))
                throw new InvalidOperationException($"Slug '{normalized}' đã được sử dụng cho loại '{entityType}'.");

            _context.SlugLookups.Add(new SlugLookup
            {
                Slug = normalized,
                EntityType = entityType,
                EntityId = entityId,
                RewritePath = rewritePath,
                RewriteQuery = rewriteQuery
            });
        }
    }

    private static string Normalize(string? slug) => (slug ?? string.Empty).Trim().ToLowerInvariant();
}
