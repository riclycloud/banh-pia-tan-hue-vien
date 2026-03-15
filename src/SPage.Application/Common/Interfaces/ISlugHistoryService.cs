namespace SPage.Application.Common.Interfaces;

public interface ISlugHistoryService
{
    /// <summary>
    /// Truy vấn lịch sử slug. Trả về redirect path nếu tìm thấy, null nếu không.
    /// Kết quả được cache tự động.
    /// </summary>
    Task<string?> GetRedirectPathAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi nhận việc thay đổi slug. Gọi từ UpdatePost/Product/Page command.
    /// </summary>
    Task RecordAsync(
        string oldSlug,
        string newSlug,
        string entityType,
        int entityId,
        string redirectPath,
        string? changedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Xóa cache sau khi có thay đổi slug mới</summary>
    Task InvalidateCacheAsync(string slug);
}
