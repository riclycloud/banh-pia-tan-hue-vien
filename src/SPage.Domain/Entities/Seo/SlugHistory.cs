namespace SPage.Domain.Entities.Seo;

/// <summary>
/// Lưu lịch sử thay đổi slug để middleware tự động 301 redirect.
/// Khi admin đổi slug bài viết/sản phẩm/trang, slug cũ được ghi vào đây.
/// </summary>
public sealed class SlugHistory
{
    public int Id { get; set; }

    /// <summary>Slug cũ đã bị thay thế</summary>
    public string OldSlug { get; set; } = string.Empty;

    /// <summary>Slug mới hiện tại (canonical URL đích)</summary>
    public string NewSlug { get; set; } = string.Empty;

    /// <summary>Loại entity: post | product | page</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID của entity gốc</summary>
    public int EntityId { get; set; }

    /// <summary>URL đích đầy đủ để redirect (relative path)</summary>
    public string RedirectPath { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ChangedBy { get; set; }
}
