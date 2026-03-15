using SPage.Domain.Common;

namespace SPage.Domain.Entities.Media;

/// <summary>Đại diện cho một file hình ảnh được upload và lưu trong thư viện media.</summary>
public sealed class MediaFile : BaseAuditableEntity
{
    /// <summary>Tên file gốc do người dùng upload, e.g. "photo.jpg"</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Tên file thực tế lưu trên disk (GUID-based), e.g. "a1b2c3d4.jpg"</summary>
    public string StoredName { get; set; } = string.Empty;

    /// <summary>Đường dẫn URL công khai, e.g. "/uploads/images/a1b2c3d4.jpg"</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Kích thước file tính theo bytes</summary>
    public long FileSize { get; set; }

    /// <summary>MIME type, e.g. "image/jpeg"</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Văn bản thay thế (alt text) cho SEO / accessibility</summary>
    public string? AltText { get; set; }
}
