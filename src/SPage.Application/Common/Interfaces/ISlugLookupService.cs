namespace SPage.Application.Common.Interfaces;

/// <summary>
/// Đồng bộ và tra cứu bảng SlugLookups (một bảng slug cho Post, Category, ProductCategory, Product).
/// </summary>
public interface ISlugLookupService
{
    /// <summary>Trả về true nếu slug đã được sử dụng bởi entity khác (hoặc bất kỳ nếu exclude null).</summary>
    Task<bool> IsSlugInUseAsync(
        string slug,
        string? excludeEntityType = null,
        int? excludeEntityId = null,
        CancellationToken cancellationToken = default);

    Task SyncPostAsync(string? slug, int postId, CancellationToken cancellationToken = default);
    Task SyncCategoryAsync(string? slug, int categoryId, CancellationToken cancellationToken = default);
    Task SyncProductCategoryAsync(string? slug, int categoryId, CancellationToken cancellationToken = default);
    Task SyncProductAsync(string? slug, int productId, string categorySlug, CancellationToken cancellationToken = default);

    Task RemoveAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
}
