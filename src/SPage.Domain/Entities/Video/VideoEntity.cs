using SPage.Domain.Common;
using SPage.Domain.Enums;

namespace SPage.Domain.Entities.Video;

/// <summary>Video nhúng từ YouTube / Facebook / TikTok</summary>
public sealed class VideoEntity : BaseAuditableEntity, ISeoEntity
{
    public string Title       { get; set; } = string.Empty;
    public string Slug        { get; set; } = string.Empty;
    public string? Description{ get; set; }

    public VideoPlatform Platform  { get; set; } = VideoPlatform.YouTube;
    /// <summary>ID video trên nền tảng, e.g. "dQw4w9WgXcQ" cho YouTube</summary>
    public string VideoId    { get; set; } = string.Empty;
    /// <summary>URL gốc người dùng nhập</summary>
    public string VideoUrl   { get; set; } = string.Empty;
    /// <summary>null = auto-generate từ YouTube API; giá trị override khi nhập tay</summary>
    public string? ThumbnailUrl { get; set; }
    /// <summary>Độ dài video, e.g. "PT3M45S" (ISO 8601) hoặc "3:45"</summary>
    public string? Duration   { get; set; }
    /// <summary>Tags ngăn cách bằng dấu phẩy, e.g. "marketing,tvc,2025"</summary>
    public string? TagsCsv    { get; set; }

    public bool            IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public bool            IsFeatured  { get; set; }
    public int             ViewCount   { get; set; }

    public int?          CategoryId { get; set; }
    public VideoCategory? Category  { get; set; }

    // ISeoEntity
    public string? MetaTitle       { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl    { get; set; }
    public bool    IsIndexable     { get; set; } = true;

    /// <summary>Trả về URL embed tương ứng với từng nền tảng</summary>
    public string GetEmbedUrl() => Platform switch
    {
        VideoPlatform.YouTube  => $"https://www.youtube.com/embed/{VideoId}?rel=0&modestbranding=1",
        VideoPlatform.Facebook => $"https://www.facebook.com/plugins/video.php?href={Uri.EscapeDataString(VideoUrl)}&show_text=0",
        VideoPlatform.TikTok   => $"https://www.tiktok.com/embed/v2/{VideoId}",
        _                      => VideoUrl
    };

    /// <summary>URL thumbnail: auto cho YouTube, nhập tay cho các platform khác</summary>
    public string GetThumbnailUrl() =>
        !string.IsNullOrEmpty(ThumbnailUrl)
            ? ThumbnailUrl
            : Platform == VideoPlatform.YouTube
                ? $"https://img.youtube.com/vi/{VideoId}/maxresdefault.jpg"
                : "/img/video-placeholder.svg";
}
